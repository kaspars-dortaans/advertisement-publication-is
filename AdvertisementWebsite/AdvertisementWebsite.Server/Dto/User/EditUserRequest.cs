using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.User;

public class EditUserRequest : EditUserInfo
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public int UserId { get; set; }
    public IEnumerable<string> UserRoles { get; set; } = default!;
}
