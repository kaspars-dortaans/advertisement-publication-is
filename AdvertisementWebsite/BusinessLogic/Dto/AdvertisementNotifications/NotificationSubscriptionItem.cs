using BusinessLogic.Enums;

namespace BusinessLogic.Dto.AdvertisementNotifications;

public class NotificationSubscriptionItem
{
    public int Id { get; set; }
    public string? OwnerUsername { get; set; }
    public string Title { get; set; } = default!;
    public IEnumerable<string> Keywords { get; set; } = default!;
    public PaymentSubjectStatus Status { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? ValidToDate { get; set; }
    public string CategoryName { get; set; } = default!;
}
