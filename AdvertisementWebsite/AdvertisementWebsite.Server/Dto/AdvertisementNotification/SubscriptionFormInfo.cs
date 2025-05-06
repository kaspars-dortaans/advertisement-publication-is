using BusinessLogic.Dto.Category;

namespace AdvertisementWebsite.Server.Dto.AdvertisementNotification;

public class SubscriptionFormInfo
{
    public CreateOrEditNotificationSubscriptionRequest Subscription { get; set; } = default!;
    public CategoryAttributeListData CategoryInfo { get; set; } = default!;
}
