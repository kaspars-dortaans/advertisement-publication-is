using System.ComponentModel.DataAnnotations;

namespace Web.Dto.Advertisement;

public class ReportAdvertisementRequest
{
    [Required]
    public string Description { get; set; } = default!;
    [Required]
    public int ReportedAdvertisementId { get; set; }
}
