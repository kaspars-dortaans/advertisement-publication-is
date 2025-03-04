using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace Web.Validators;

public class AllowedFileTypes : ValidationAttribute
{
    private readonly string _allowedFileTypes;

    public AllowedFileTypes(string allowedFileTypes) : base()
    {
        _allowedFileTypes = allowedFileTypes;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is not IFormFile formFile)
        {
            return new ValidationResult(CustomErrorCodes.InvalidFileExtension);
        }

        var formFileExtension = formFile.FileName.Split('.').Last();
        var allowedExtensions = _allowedFileTypes
            .Split(',')
            .Select(ext => ext.TrimStart(new char[] { '.', ' ' }));

        if (allowedExtensions.Contains(formFileExtension))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(CustomErrorCodes.DisallowedFileType);
    }
}
