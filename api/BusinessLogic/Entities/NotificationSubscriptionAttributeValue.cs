namespace BusinessLogic.Entities;

public class NotificationSubscriptionAttributeValue
{
    public int Id {  get; set; }
    public string Value { get; set; } = default!;
    public int SubscriptionId { get; set; }
    public AdvertisementNotificationSubscription Subscription { get; set; } = default!;
    public int AttributeId { get; set; }
    public Attribute Attribute { get; set;} = default!;
}
