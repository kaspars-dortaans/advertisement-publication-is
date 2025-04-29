namespace BusinessLogic.Entities;

public class AdvertisementNotificationSubscription
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string[]? Keywords { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ValidTo { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public int OwnerId { get; set; }
    public User Owner { get; set; } = default!;
    public IEnumerable<NotificationSubscriptionAttributeValue> AttributeFilters { get; set; } = default!;
}
