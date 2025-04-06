using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace Web.Validators;

public class AllowedFileTypes(string allowedFileTypes) : ValidationAttribute()
{
    private readonly string _allowedFileTypes = allowedFileTypes;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        List<IFormFile> files;

        if (value is IEnumerable<IFormFile> fileEnumerable)
        {
            files = fileEnumerable.ToList();
        }
        else if (value is IFormFile formFile)
        {
            files = [formFile];
        }
        else
        {
            return new ValidationResult(CustomErrorCodes.InvalidFile);
        }

        var allowedExtensions = _allowedFileTypes
            .Split(',')
            .Select(ext => ext.TrimStart(['.', ' ']));

        var invalidFileNames = files
            .Select((f, i) => new {File = f, Index = i})
            .Where(e => !allowedExtensions.Contains(e.File.FileName.Split('.').Last()))
            .Select(e => $"{(value is IFormFile? "" : $"[{e.Index}].")}{e.File.FileName.Replace(".", "\\.")}");

        return invalidFileNames.Any()
            ? new ValidationResult(CustomErrorCodes.InvalidFileExtension, [string.Join(",", invalidFileNames)])
            : ValidationResult.Success;
    }
}
