using BusinessLogic.Dto.Image;

namespace Web.Dto.User;

public class UserInfo
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public bool IsPhoneNumberPublic { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsEmailPublic { get; set; }
    public string Email { get; set; } = default!;
    public string? LinkToUserSite { get; set; }
    public ImageUrl ProfileImageUrl { get; set; } = default!;
}
