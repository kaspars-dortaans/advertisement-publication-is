using BusinessLogic.Constants;
using BusinessLogic.Dto.Time;
using System.ComponentModel.DataAnnotations;

namespace Web.Dto.Advertisement;

public class ExtendAdvertisementRequest
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public IEnumerable<int> AdvertisementIds { get; set; } = [];

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public PostTimeDto ExtendTime { get; set; } = default!;
}
