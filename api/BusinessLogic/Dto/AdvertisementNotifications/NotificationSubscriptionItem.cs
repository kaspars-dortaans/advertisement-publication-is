namespace BusinessLogic.Dto.AdvertisementNotifications;

public class NotificationSubscriptionItem
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Keywords { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ValidTo { get; set; }
    public string CategoryName { get; set; } = default!;
}
