using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Dto.User;

public class EditUserInfo
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public bool IsPhoneNumberPublic { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsEmailPublic { get; set; }
    public string Email { get; set; } = default!;
    public string? LinkToUserSite { get; set; }
    public bool ProfileImageChanged { get; set; }
    public IFormFile? ProfileImage { get; set; }
}
