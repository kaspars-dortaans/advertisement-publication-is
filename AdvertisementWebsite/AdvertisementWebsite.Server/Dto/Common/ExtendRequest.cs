using BusinessLogic.Constants;
using BusinessLogic.Dto.Time;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Common;

public class ExtendRequest
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public IEnumerable<int> Ids { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public PostTimeDto ExtendTime { get; set; } = default!;
}
