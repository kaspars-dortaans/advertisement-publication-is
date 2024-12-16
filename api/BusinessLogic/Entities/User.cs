using Microsoft.AspNetCore.Identity;

namespace BusinessLogic.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsPhoneNumberPublic { get; set; }
    public bool IsEmailPublic { get; set; }
    public int? ProfileImageFileId { get; set; }
    public File ProfileImageFile { get; set; } = default!;
}
