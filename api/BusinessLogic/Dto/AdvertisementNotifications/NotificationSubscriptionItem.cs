namespace BusinessLogic.Dto.AdvertisementNotifications;

public class NotificationSubscriptionItem
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public IEnumerable<string> Keywords { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ValidTo { get; set; }
    public string CategoryName { get; set; } = default!;
}
