using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace Web.Validators;

public class MaxFileSize(int maxFileSizeInBytes) : ValidationAttribute()
{
    private readonly int _maxFileSizeInBytes = maxFileSizeInBytes;

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

        //TODO: Return appropriate data size unit based on max size value
        var maxFileSizeInMb = $"{_maxFileSizeInBytes / 1024 / 1024} MB";

        var invalidFileNames = files
            .Select((f, i) => new { Index = i, File = f})
            .Where(e => e.File.Length > _maxFileSizeInBytes)
            .Select(e => $"{(value is IFormFile ? "" : $"[{e.Index}].")}{e.File.FileName.Replace(".", "\\.")}.{maxFileSizeInMb}");

        return invalidFileNames.Any()
            ? new ValidationResult(CustomErrorCodes.InvalidSizeForFile, [string.Join(",", invalidFileNames)])
            : ValidationResult.Success;
    }

}
