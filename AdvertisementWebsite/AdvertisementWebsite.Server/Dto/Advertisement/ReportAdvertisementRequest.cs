using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Advertisement;

public class ReportAdvertisementRequest
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Description { get; set; } = default!;
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public int ReportedAdvertisementId { get; set; }
}
