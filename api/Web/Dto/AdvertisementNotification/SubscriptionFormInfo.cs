using BusinessLogic.Dto.Advertisement;

namespace Web.Dto.AdvertisementNotification;

public class SubscriptionFormInfo
{
    public CreateOrEditNotificationSubscriptionRequest Subscription { get; set; } = default!;
    public CategoryFormInfo CategoryInfo { get; set; } = default!;
}
