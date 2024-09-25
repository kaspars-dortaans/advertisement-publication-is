using Microsoft.AspNetCore.Identity;

namespace api.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsPhoneNumberPublic { get; set; }
    public bool IsEmailPublic { get; set; }
}
