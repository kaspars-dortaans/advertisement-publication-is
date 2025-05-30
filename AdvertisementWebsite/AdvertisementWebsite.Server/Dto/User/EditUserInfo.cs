using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;
using AdvertisementWebsite.Server.Validators;

namespace AdvertisementWebsite.Server.Dto.User;

public class EditUserInfo
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    [EmailAddress(ErrorMessage = CustomErrorCodes.NotAnEmail)]
    [MaxLength(UserConstants.MaxEmailLength, ErrorMessage = CustomErrorCodes.MaxLength)]
    public string Email { get; set; } = default!;

    public bool IsEmailPublic { get; set; }

    [MaxLength(UserConstants.MaxNameLength, ErrorMessage = CustomErrorCodes.MaxLength)]
    [MinLength(UserConstants.MinNameLength, ErrorMessage = CustomErrorCodes.MinLength)]
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string FirstName { get; set; } = default!;

    [MaxLength(UserConstants.MaxNameLength, ErrorMessage = CustomErrorCodes.MaxLength)]
    [MinLength(UserConstants.MinNameLength, ErrorMessage = CustomErrorCodes.MinLength)]
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string LastName { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string UserName { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    [MaxLength(UserConstants.MaxEmailLength, ErrorMessage = CustomErrorCodes.MaxLength)]
    [Phone(ErrorMessage = CustomErrorCodes.NotAPhoneNumber)]
    public string PhoneNumber { get; set; } = default!;

    public bool IsPhoneNumberPublic { get; set; }

    [Url(ErrorMessage = CustomErrorCodes.NotAUrl)]
    [MaxLength(UserConstants.MaxUrlLength, ErrorMessage = CustomErrorCodes.MaxLength)]
    public string? LinkToUserSite { get; set; }

    public bool ProfileImageChanged { get; set; }

    [MaxFileSize(ImageConstants.MaxFileSizeInBytes)]
    [AllowedFileTypes(ImageConstants.AllowedFileTypes)]
    [AllowedImageAspectRatio(ImageConstants.AllowedAspectRatio)]
    public IFormFile? ProfileImage { get; set; }
}
