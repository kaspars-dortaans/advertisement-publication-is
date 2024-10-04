using api.Constants;
using System.ComponentModel.DataAnnotations;

namespace api.Validators;

public class MaxFileSize : ValidationAttribute
{
    private readonly int _maxFileSizeInBits;

    public MaxFileSize(int maxFileSizeInBits) : base()
    {
        _maxFileSizeInBits = maxFileSizeInBits;
    }

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
