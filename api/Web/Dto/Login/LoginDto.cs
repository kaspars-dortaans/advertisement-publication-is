using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace Web.Dto.Login;

public class LoginDto
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Password { get; set; } = default!;
}
