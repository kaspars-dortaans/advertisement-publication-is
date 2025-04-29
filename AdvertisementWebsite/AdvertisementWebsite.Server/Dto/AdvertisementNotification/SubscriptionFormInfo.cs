using BusinessLogic.Dto.Advertisement;

namespace AdvertisementWebsite.Server.Dto.AdvertisementNotification;

public class SubscriptionFormInfo
{
    public CreateOrEditNotificationSubscriptionRequest Subscription { get; set; } = default!;
    public CategoryFormInfo CategoryInfo { get; set; } = default!;
}
