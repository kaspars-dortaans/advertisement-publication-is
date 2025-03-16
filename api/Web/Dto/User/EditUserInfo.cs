using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;
using Web.Validators;

namespace Web.Dto.User;

public class EditUserInfo
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    [EmailAddress(ErrorMessage = CustomErrorCodes.NotAnEmail)]
    public string Email { get; set; } = default!;

    public bool IsEmailPublic { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string LastName { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string UserName { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    [Phone(ErrorMessage = CustomErrorCodes.NotAPhoneNumber)]
    public string PhoneNumber { get; set; } = default!;

    public bool IsPhoneNumberPublic { get; set; }

    public string? LinkToUserSite { get; set; }

    public bool ProfileImageChanged { get; set; }

    [MaxFileSize(ProfileImageConstants.MaxFileSizeInBytes)]
    [AllowedFileTypes(ProfileImageConstants.AllowedFileTypes)]
    [AllowedImageAspectRatio(ProfileImageConstants.AllowedAspectRatio)]
    public IFormFile? ProfileImage { get; set; }
}
