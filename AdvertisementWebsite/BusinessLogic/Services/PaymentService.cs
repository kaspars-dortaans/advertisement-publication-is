using BusinessLogic.Constants;
using BusinessLogic.Dto.Payment;
using BusinessLogic.Entities;
using BusinessLogic.Entities.Payments;
using BusinessLogic.Enums;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers.BackgroundJobs;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace BusinessLogic.Services;

public class PaymentService(Context dbContext) : BaseService<Payment>(dbContext), IPaymentService
{
    public async Task<PriceInfo> GetPriceInfo(IEnumerable<PaymentItemDto> items, int userId)
    {
        //Assign prices
        var (total, advertisementIds, subscriptionIds) = await ProcessPaymentItems(items);
        var advertisementTitlesFuture = DbContext.Advertisements
            .Where(a => advertisementIds.Contains(a.Id) && a.OwnerId == userId)
            .Select(a => new { a.Id, a.Title })
            .Future();

        var subscriptionTitlesFuture = DbContext.NotificationSubscriptions
            .Where(s => subscriptionIds.Contains(s.Id) && s.OwnerId == userId)
            .Select(s => new { s.Id, s.Title })
            .Future();

        //Execute queries in one roundtrip
        var advertisementTitles = await advertisementTitlesFuture.ToListAsync();
        var subscriptionTitles = subscriptionTitlesFuture.ToList();

        //Assign titles
        var notFoundItems = new List<PaymentItemDto>();
        foreach (var item in items)
        {
            string? title;
            if (item.Type == PaymentType.CreateAdvertisement || item.Type == PaymentType.ExtendAdvertisement)
            {
                title = advertisementTitles.FirstOrDefault(a => a.Id == item.PaymentSubjectId)?.Title;
            }
            else
            {
                title = subscriptionTitles.FirstOrDefault(s => s.Id == item.PaymentSubjectId)?.Title;
            }

            if(title == null)
            {
                notFoundItems.Add(item);
            } else
            {
                item.Title = title;
            }
        }

        return new PriceInfo
        {
            TotalAmount = total,
            Items = items.Where(i => !notFoundItems.Contains(i))
        };
    }

    public async Task MakePayment(IEnumerable<PaymentItemDto> items, decimal confirmTotalAmount, int userId)
    {
        /* 
         * Note: Currently payment page is just a mockup, payment processing Api should be called before saving payment.
         *       Logic refactoring will be required, when integrating payment Api.
         */

        //Make sure totals match
        var (total, advertisementIds, subscriptionIds) = await ProcessPaymentItems(items);
        if(total != confirmTotalAmount)
        {
            throw new ApiException([CustomErrorCodes.TotalsDoesNotMatch]);
        }

        var advertisementsFuture = DbContext.Advertisements.Where(a => advertisementIds.Contains(a.Id)).Future();
        var subscriptionsFuture = DbContext.NotificationSubscriptions.Where(s => subscriptionIds.Contains(s.Id)).Future();

        //Execute queries in one roundtrip
        var advertisements = await advertisementsFuture.ToListAsync();
        var subscriptions = subscriptionsFuture.ToList();

        var createdAdvertisementIds = new List<int>();

        //Update advertisements and subscriptions
        foreach(var item in items)
        {
            bool createAction = item.Type == PaymentType.CreateAdvertisement || item.Type == PaymentType.CreateAdvertisementNotificationSubscription;
            IPaymentItemSubject paymentItemSubject;
            if(item.Type == PaymentType.CreateAdvertisement || item.Type == PaymentType.ExtendAdvertisement)
            {
                paymentItemSubject = advertisements.First(a => a.Id == item.PaymentSubjectId);
                if (createAction)
                {
                    createdAdvertisementIds.Add(paymentItemSubject.Id);
                }
            } else
            {
                paymentItemSubject = subscriptions.First(s => s.Id == item.PaymentSubjectId);
            }

            item.Title = paymentItemSubject.Title;

            if (createAction)
            {
                paymentItemSubject.CreatedDate = DateTime.UtcNow;
                paymentItemSubject.ValidToDate = DateTime.UtcNow.AddDays(item.TimePeriod.ToDays());
            }
            else
            {
                paymentItemSubject.CreatedDate ??= DateTime.UtcNow;
                paymentItemSubject.ValidToDate = (paymentItemSubject.ValidToDate != null && paymentItemSubject.ValidToDate > DateTime.UtcNow)
                    ? paymentItemSubject.ValidToDate.Value.AddDays(item.TimePeriod.ToDays())
                    : DateTime.UtcNow.AddDays(item.TimePeriod.ToDays());
            }
        }

        //Add payment
        DbSet.Add(new Payment
        {
            Amount = total,
            PayerId = userId,
            Items = items.Select(i => new PaymentItem
            {
                PaymentSubjectId = i.PaymentSubjectId,
                Price = i.Price,
                Title = i.Title,
                Type = i.Type,
                TimePeriodInDays = i.TimePeriod.ToDays(),
            }).ToList()
        });

        //Save changes
        await DbContext.SaveChangesAsync();

        //Send advertisement notifications
        foreach(var id in createdAdvertisementIds)
        {
            BackgroundJob.Enqueue<IAdvertisementNotificationSender>((s) => s.SendNotifications(id));
        }
    }

    /// <summary>
    /// Fills payment item prices, calculate total price, adds advertisement and subscription ids to provided lists
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    private async Task<(decimal, List<int>, List<int>)> ProcessPaymentItems(IEnumerable<PaymentItemDto> items)
    {
        var advertisementIds = new List<int>();
        var subscriptionIds = new List<int>();
        var total = 0m;
        var costs = await DbContext.Costs.AsNoTracking()
            .GroupBy(c => c.Type)
            .Select(g => g.First())
            .ToDictionaryAsync(c => c.Type, c => c.Amount);

        foreach (var item in items)
        {
            switch (item.Type)
            {
                case PaymentType.CreateAdvertisement:
                    item.Price = CalculateNewAdvertisementPrice(costs, item.TimePeriod.ToDays());
                    advertisementIds.Add(item.PaymentSubjectId);
                    break;
                case PaymentType.ExtendAdvertisement:
                    item.Price = CalculateExtendAdvertisementPrice(costs, item.TimePeriod.ToDays());
                    advertisementIds.Add(item.PaymentSubjectId);
                    break;
                case PaymentType.CreateAdvertisementNotificationSubscription:
                    item.Price = CalculateNewSubscriptionPrice(costs, item.TimePeriod.ToDays());
                    subscriptionIds.Add(item.PaymentSubjectId);
                    break;
                case PaymentType.ExtendAdvertisementNotificationSubscription:
                    item.Price = CalculateExtendSubscriptionPrice(costs, item.TimePeriod.ToDays());
                    subscriptionIds.Add(item.PaymentSubjectId);
                    break;
                default:
                    throw new ArgumentException("Unexpected PaymentType value '" + item.Type + "' for payment item", nameof(items));
            };
            total += item.Price;
        }
        return (total, advertisementIds, subscriptionIds);
    }

    /// <summary>
    /// Calculate price for creating new advertisement and publicly displaying it for specified time
    /// </summary>
    /// <param name="costs"></param>
    /// <param name="timePeriodInDays"></param>
    /// <returns></returns>
    private static decimal CalculateNewAdvertisementPrice(Dictionary<CostType, decimal> costs, int timePeriodInDays)
    {
        return costs[CostType.CreateAdvertisement] + (costs[CostType.AdvertisementPerDay] * timePeriodInDays);
    }

    /// <summary>
    /// Calculate price for extending existing time for which advertisement is publicly visible
    /// </summary>
    /// <param name="costs"></param>
    /// <param name="timePeriodInDays"></param>
    /// <returns></returns>
    private static decimal CalculateExtendAdvertisementPrice(Dictionary<CostType, decimal> costs, int timePeriodInDays)
    {
        return costs[CostType.AdvertisementPerDay] * timePeriodInDays;
    }

    /// <summary>
    /// Calculate price for creating new subscription for advertisement notifications
    /// </summary>
    /// <param name="costs"></param>
    /// <param name="timePeriodInDays"></param>
    /// <returns></returns>
    private static decimal CalculateNewSubscriptionPrice(Dictionary<CostType, decimal> costs, int timePeriodInDays)
    {
        return costs[CostType.CreateAdvertisementNotificationSubscription] + (costs[CostType.SubscriptionPerDay] * timePeriodInDays);
    }

    /// <summary>
    /// Calculate price for extending existing subscription 
    /// </summary>
    /// <param name="costs"></param>
    /// <param name="timePeriodInDays"></param>
    /// <returns></returns>
    private static decimal CalculateExtendSubscriptionPrice(Dictionary<CostType, decimal> costs, int timePeriodInDays)
    {
        return costs[CostType.SubscriptionPerDay] * timePeriodInDays;
    }
}
