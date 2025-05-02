using BusinessLogic.Dto.Time;

namespace BusinessLogic.Dto.AdvertisementNotifications;

public class CreateOrEditSubscription
{
    public int? Id { get; set; }
    public string Title { get; set; } = default!;
    public IEnumerable<string>? Keywords { get; set; }
    public PostTimeDto? PaidTime { get; set; }
    public DateTime? ValidToDate { get; set; }
    public int CategoryId { get; set; }
    public IEnumerable<KeyValuePair<int, string>> AttributeValues { get; set; } = default!;
}
