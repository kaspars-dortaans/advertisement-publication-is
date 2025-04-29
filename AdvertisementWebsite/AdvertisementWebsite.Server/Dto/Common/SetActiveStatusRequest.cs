using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Common;

public class SetActiveStatusRequest
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public IEnumerable<int> Ids { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public bool IsActive { get; set; }
}
