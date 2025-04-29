using BusinessLogic.Dto.Email;
using BusinessLogic.Entities;
using BusinessLogic.Enums;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.BackgroundJobs;
using BusinessLogic.Helpers.EmailClient;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MimeKit.Text;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using AdvertisementWebsite.Server.Helpers;

namespace AdvertisementWebsite.Server.BackgroundJobs;

public class AdvertisementNotificationSender(
    Context dbContext,
    IEmailClient emailClient,
    IStringLocalizer<AdvertisementNotificationSender> localizer,
    IConfiguration configuration,
    LinkGenerator linkGenerator,
    IServer server,
    ILogger<AdvertisementNotificationSender> logger
) : IAdvertisementNotificationSender
{
    private readonly Context _dbContext = dbContext;
    private readonly IStringLocalizer<AdvertisementNotificationSender> _localizer = localizer;
    private readonly IConfiguration _configuration = configuration;
    private readonly IEmailClient _emailClient = emailClient;
    private readonly LinkGenerator _linkGenerator = linkGenerator;
    private readonly ILogger _logger = logger;
    private readonly string? _baseUrl = server.Features.GetRequiredFeature<IServerAddressesFeature>()?.Addresses.First();

    /// <summary>
    /// Send notifications to users with notification subscriptions that match newly created advertisement
    /// </summary>
    /// <param name="newAdvertisementId"></param>
    /// <returns></returns>
    public async Task SendNotifications(int newAdvertisementId)
    {
        if(string.IsNullOrEmpty(_baseUrl))
        {
            _logger.LogError("Server host string is null or empty");
            throw new NullReferenceException();
        }

        //Filter subscriptions
        var newAdvertisement = await _dbContext.Advertisements
            .AsNoTracking()
            .Include(a => a.AttributeValues)
            .ThenInclude(av => av.Attribute)
            .FirstAsync(a => a.Id == newAdvertisementId);

        var userSubscriptionData = await _dbContext.NotificationSubscriptions
            .Where(GetNotificationFilterPredicate(newAdvertisement))
            .Select(s => new { 
                s.Owner.Email, 
                s.Owner.UserName, 
                SubscriptionTitle = s.Title, 
                Language = _dbContext.UserClaims.FirstOrDefault(c => c.UserId == s.OwnerId && c.ClaimType == ClaimTypes.Locality) 
            })
            .ToListAsync();

        //Create email contents
        var subscriptionsByUser = userSubscriptionData
            .GroupBy(d => d.Email)
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .Select(g => new { 
                Email = g.Key!, 
                SubscriptionTitles = g.Select(d => d.SubscriptionTitle).ToList(), 
                UserName = g.First().UserName!, 
                Language = g.First().Language?.ClaimValue });

        if (!subscriptionsByUser.Any())
        {
            return;
        }

        //Get email template
        var assembly = this.GetType().Assembly;
        var templatePath = assembly.GetName().Name + ".BackgroundJobs.AdvertisementNotificationEmailTemplate.html";
        using var templateStream = assembly.GetManifestResourceStream(templatePath);
        var reader = new StreamReader(templateStream!);
        var template = reader.ReadToEnd();

        //Image url
        var baseUrl = _configuration.GetValue<string>("Frontend:BaseUrl") ?? "";
        var logoUrl = baseUrl + _configuration.GetValue<string>("Frontend:LogoUrl");
        var viewAdvertisementUrl = baseUrl + string.Format(
            _configuration.GetValue<string>("Frontend:ViewAdvertisementUrl") ?? "",
            newAdvertisement.Id);

        var advertisementThumbnailUrl = FileUrlHelper.GetThumbnailUrl(_linkGenerator, _baseUrl, newAdvertisement.Id);

        var sendEmailTasks = new List<Task>();

        //Original cultures
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        var originalUiCulture = Thread.CurrentThread.CurrentUICulture;
        try
        {
            //Send advertisement notification email to users
            foreach (var emailDto in subscriptionsByUser)
            {
                Thread.CurrentThread.CurrentCulture = string.IsNullOrEmpty(emailDto.Language) 
                    ? originalCulture : new CultureInfo(emailDto.Language);

                Thread.CurrentThread.CurrentUICulture = string.IsNullOrEmpty(emailDto.Language) 
                    ? originalCulture : new CultureInfo(emailDto.Language);

                var subscriptionTitles = emailDto.SubscriptionTitles.Select(t => $"<li>{HtmlUtils.HtmlEncode(t)}</li>").ToList();
                var subscriptionTitleList = string.Join("\n", subscriptionTitles);
                var userEmailContent = string.Format(template,
                    logoUrl,
                    _localizer["LogoAlt"],
                    _localizer["Title"],
                    _localizer["MatchedSubscriptions"],
                    subscriptionTitleList,
                    advertisementThumbnailUrl,
                    _localizer["AdvertisementImageAlt"],
                    viewAdvertisementUrl,
                    _localizer["SeeAdvertisement"],
                    newAdvertisement.Title,
                    newAdvertisement.AdvertisementText);

                sendEmailTasks.Add(_emailClient.SendEmail(new SendEmailDto
                {
                    ReceiverEmail = emailDto.Email,
                    ReceiverName = emailDto.UserName,
                    Subject = _localizer["Title"],
                    EmailBody = userEmailContent,
                    IsBodyHtml = true
                }));
            }
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalUiCulture;
        }

        await Task.WhenAll([.. sendEmailTasks]);
    }

    private static Expression<Func<AdvertisementNotificationSubscription, bool>> GetNotificationFilterPredicate(
        Advertisement newAdvertisement
    )
    {
        //Filter subscriptions
        var titleLowerCase = newAdvertisement.Title.ToLower();
        var textLowercase = newAdvertisement.AdvertisementText.ToLower();
        var attributeIds = newAdvertisement.AttributeValues.Select(av => av.AttributeId).ToList();

        Expression<Func<AdvertisementNotificationSubscription, bool>> validSubscription = 
            s => s.IsActive && s.ValidTo > newAdvertisement.PostedDate;

        Expression<Func<AdvertisementNotificationSubscription, bool>> keywordExp = s =>
            s.Keywords == null
            || !s.Keywords.Any()
            || s.Keywords!.Any(k => titleLowerCase.Contains(k.ToLower()) || textLowercase.Contains(k.ToLower()));

        Expression<Func<AdvertisementNotificationSubscription, bool>> checkAllAttributesPresentExp = 
            s => s.AttributeFilters.All(af => attributeIds.Any(id => id == af.AttributeId));
        
        Expression<Func<AdvertisementNotificationSubscription, bool>> attributeFilterExp = 
            s => s.AttributeFilters.AsQueryable().All(MatchSubscriptionFilters(newAdvertisement.AttributeValues));
        
        return validSubscription.AndAlso(keywordExp.AndAlso(checkAllAttributesPresentExp.AndAlso(attributeFilterExp)));
    }

    /// <summary>
    /// Get expression to filter out advertisement notification subscriptions 
    /// whose attribute filters did not match advertisement attribute values
    /// </summary>
    /// <param name="attributeValues"></param>
    /// <returns></returns>
    private static Expression<Func<NotificationSubscriptionAttributeValue, bool>> MatchSubscriptionFilters(
        ICollection<AdvertisementAttributeValue> attributeValues
    )
    {
        var pe = Expression.Parameter(typeof(NotificationSubscriptionAttributeValue));
        Expression? combined = null;

        var toLowerMethod = typeof(string).GetMethod("ToLower", BindingFlags.Public | BindingFlags.Instance, [])!;
        var containsMethod = typeof(string).GetMethod("Contains", BindingFlags.Public | BindingFlags.Instance, [typeof(string)])!;


        foreach (var av in attributeValues)
        {
            var attributeIdProperty = Expression.Property(pe, nameof(NotificationSubscriptionAttributeValue.AttributeId));
            var attributeIdMatchesExp = Expression.Equal(attributeIdProperty, Expression.Constant(av.AttributeId));
            Expression attributeFilterExp = Expression.Constant(true);
            var avValueExp = Expression.Constant(av.Value);
            var afValueExp = Expression.Property(pe, nameof(NotificationSubscriptionAttributeValue.Value));
            switch (av.Attribute.ValueType)
            {
                case ValueTypes.ValueListEntry:
                case ValueTypes.Decimal:
                case ValueTypes.Integer:
                    //Check id advertisement attribute value is equal to subscription attribute value
                    attributeFilterExp = Expression.AndAlso(attributeIdMatchesExp, Expression.Equal(afValueExp, avValueExp));
                    break;

                case ValueTypes.Text:
                    //Check if advertisement attribute value contains subscription attribute value
                    var avValueLowercase = Expression.Call(avValueExp, toLowerMethod);
                    var afValueLowercase = Expression.Call(afValueExp, toLowerMethod);
                    var containsExp = Expression.Call(avValueLowercase, containsMethod, afValueLowercase);
                    attributeFilterExp = Expression.AndAlso(attributeIdMatchesExp, containsExp);
                    break;
            }

            if (combined == null)
            {
                combined = attributeFilterExp;
            }
            else
            {
                combined = Expression.OrElse(combined, attributeFilterExp);
            }
        }

        //create and return the predicate
        return Expression.Lambda<Func<NotificationSubscriptionAttributeValue, bool>>(combined ?? Expression.Constant(true), [pe]);
    }
}
