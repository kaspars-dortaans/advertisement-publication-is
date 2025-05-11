using BusinessLogic.Constants;
using BusinessLogic.Dto.AdvertisementNotifications;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Time;
using BusinessLogic.Entities;
using BusinessLogic.Enums;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.CookieSettings;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public partial class AdvertisementNotificationSubscriptionService(
    Context dbContext,
    CookieSettingsHelper cookieSettingHelper,
    IAttributeValidatorService attributeValidatorService
    ) : BaseService<AdvertisementNotificationSubscription>(dbContext), IAdvertisementNotificationSubscriptionService
{
    private readonly CookieSettingsHelper _cookieSettingHelper = cookieSettingHelper;
    private readonly IAttributeValidatorService _attributeValidatorService = attributeValidatorService;

    public async Task<DataTableQueryResponse<NotificationSubscriptionItem>> GetSubscriptions(DataTableQuery query, int userId)
    {
        var locale = _cookieSettingHelper.Settings.Locale;
        var itemQuery = Where(s => s.OwnerId == userId)
            .Select(s => new NotificationSubscriptionItem
            {
                Id = s.Id,
                Title = s.Title,
                Keywords = s.Keywords == null ? new List<string>() : s.Keywords,
                //Ef could not translate OrderBy with Localize extension method
                CategoryName = s.Category.LocalisedNames.FirstOrDefault(lt => lt.Locale == locale) != null 
                    ? s.Category.LocalisedNames.FirstOrDefault(lt => lt.Locale == locale)!.Text
                    : LocalisationConstants.NotLocalizedTextPlaceholder,
                Status = s.ValidToDate == null ? PaymentSubjectStatus.Draft
                    : (s.ValidToDate < DateTime.UtcNow
                        ? PaymentSubjectStatus.Expired
                        : s.IsActive
                            ? PaymentSubjectStatus.Active
                            : PaymentSubjectStatus.Inactive),
                CreatedDate = s.CreatedDate,
                ValidToDate = s.ValidToDate
            });
        return await DataTableQueryResolver.ResolveDataTableQuery(itemQuery, query);
    }

    public IQueryable<KeyValuePair<int, string>> GetLookupByIds(IEnumerable<int> ids, int userId)
    {
        return Where(s => s.OwnerId == userId && ids.Contains(s.Id))
            .Select(s => new KeyValuePair<int, string>(s.Id, s.Title));
    }

    public async Task<int> CreateSubscription(CreateOrEditSubscription dto, int userId)
    {
        await _attributeValidatorService.ValidateAttributeCategory(dto.CategoryId, nameof(CreateOrEditSubscription.CategoryId));
        await _attributeValidatorService.ValidateAdvertisementAttributeValues(dto.AttributeValues, dto.CategoryId, nameof(CreateOrEditSubscription.AttributeValues));

        var filterValues = dto.AttributeValues.Select(av => new NotificationSubscriptionAttributeValue
        {
            AttributeId = av.Key,
            Value = av.Value,
        });

        var subscription = await AddAsync(new AdvertisementNotificationSubscription
        {
            Title = dto.Title,
            Keywords = dto.Keywords?.ToArray() ?? [],
            IsActive = true,
            CategoryId = dto.CategoryId,
            OwnerId = userId,
            AttributeFilters = filterValues.ToList()
        });

        return subscription.Id;
    }

    public async Task<CreateOrEditSubscription> GetSubscriptionInfo(int subscriptionId, int userId)
    {
        return (await Where(s => s.Id == subscriptionId && s.OwnerId == userId)
            .Select(s => new CreateOrEditSubscription
            {
                Id = s.Id,
                CategoryId = s.CategoryId,
                Keywords = s.Keywords,
                Title = s.Title,
                ValidToDate = s.ValidToDate,
                AttributeValues = s.AttributeFilters.Select(av => new KeyValuePair<int, string>(av.AttributeId, av.Value))
            })
            .FirstOrDefaultAsync()) ?? throw new ApiException([CustomErrorCodes.NotFound]);
    }

    public async Task EditSubscription(CreateOrEditSubscription dto, int userId)
    {
        await _attributeValidatorService.ValidateAdvertisementAttributeValues(dto.AttributeValues, dto.CategoryId, nameof(CreateOrEditSubscription.AttributeValues));

        var subscription = await DbSet.Include(s => s.AttributeFilters).FirstAsync(s => s.Id == dto.Id);

        if (subscription.CategoryId != dto.CategoryId)
        {
            await _attributeValidatorService.ValidateAttributeCategory(dto.CategoryId, nameof(CreateOrEditSubscription.CategoryId));
        }

        var newFilterValues = new List<NotificationSubscriptionAttributeValue>();
        foreach (var filterValue in dto.AttributeValues)
        {
            var existingFilterValue = subscription.AttributeFilters.FirstOrDefault(av => av.AttributeId == filterValue.Key);
            if (existingFilterValue is not null)
            {
                existingFilterValue.Value = filterValue.Value;
                newFilterValues.Add(existingFilterValue);
            }
            else
            {
                newFilterValues.Add(new NotificationSubscriptionAttributeValue
                {
                    AttributeId = filterValue.Key,
                    Value = filterValue.Value,
                });
            }
        }

        subscription.Title = dto.Title;
        subscription.Keywords = dto.Keywords?.ToArray();
        subscription.CategoryId = dto.CategoryId;
        subscription.AttributeFilters = newFilterValues;

        await DbContext.SaveChangesAsync();
    }

    public async Task ExtendSubscriptions(IEnumerable<int> subscriptionIds, PostTimeDto time)
    {
        var extendDays = time.ToDays();
        var subscriptions = await Where(s => subscriptionIds.Contains(s.Id)).ToListAsync();
        foreach (var subscription in subscriptions)
        {
            if (subscription.ValidToDate != null && subscription.ValidToDate > DateTime.UtcNow)
            {
                subscription.ValidToDate = subscription.ValidToDate.Value.AddDays(extendDays);
            }
            else
            {
                subscription.ValidToDate = DateTime.UtcNow.AddDays(extendDays);
            }
        }
        await DbContext.SaveChangesAsync();
    }
}
