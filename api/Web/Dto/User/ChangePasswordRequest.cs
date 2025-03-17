using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;
using Web.Validators;

namespace Web.Dto.User;

public class ChangePasswordRequest
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string CurrentPassword { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    [EqualToProperty(nameof(Password))]
    public string ConfirmPassword { get; set; } = default!;
}
