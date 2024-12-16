using BusinessLogic.Constants;
using ImageMagick;
using System.ComponentModel.DataAnnotations;

namespace Web.Validators;

public class AllowedImageAspectRatio(double allowedImageAspectRation) : ValidationAttribute
{
    private readonly double _allowedImageAspectRation = allowedImageAspectRation;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is not IFormFile formFile)
        {
            return new ValidationResult(CustomErrorCodes.InvalidFile);
        }

        try
        {
            var info = new MagickImageInfo(formFile.OpenReadStream());
            var aspectRatio = (double)info.Width / info.Height;
            return _allowedImageAspectRation == aspectRatio ? ValidationResult.Success : new ValidationResult(CustomErrorCodes.DisallowedAspectRatio);
        } catch (MagickErrorException)
        {
            return new ValidationResult(CustomErrorCodes.InvalidImage);
        }
    }
}
