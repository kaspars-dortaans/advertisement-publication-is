using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Permission;

public class PutPermissionRequest
{
    public int? Id { get; set; }


    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Name { get; set; } = default!;
}
