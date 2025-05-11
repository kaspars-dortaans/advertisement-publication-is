using BusinessLogic.Enums;

namespace BusinessLogic.Dto.Advertisement;

public class AdvertisementInfo
{
    public int Id { get; set; }
    public string? OwnerUsername { get; set; }
    public string Title { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public PaymentSubjectStatus Status { get; set; }
    public DateTime? ValidToDate { get; set; }
    public DateTime? CreatedAtDate { get; set; }
}
