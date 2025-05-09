using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.User;

public class CreateUserRequest : RegisterDto
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public IEnumerable<string> UserRoles { get; set; } = default!;
}
