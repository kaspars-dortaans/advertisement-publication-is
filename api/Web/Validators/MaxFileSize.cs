using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace Web.Validators;

public class MaxFileSize(int maxFileSizeInBits) : ValidationAttribute()
{
    private readonly int _maxFileSizeInBits = maxFileSizeInBits;

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

        return formFile.Length > _maxFileSizeInBits 
            ? new ValidationResult(CustomErrorCodes.FileSizeIsTooLarge) 
            : ValidationResult.Success;
    }

}
