using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Validators;

public class ElementLength(int maxElementLength) : ValidationAttribute()
{
    private readonly int _maxElementLength = maxElementLength;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) { 
        
        if(value is not IEnumerable<string> strings)
        {
            return ValidationResult.Success;
        }

        return strings.All(s => s.Length <= _maxElementLength)
            ? ValidationResult.Success
            : new ValidationResult(CustomErrorCodes.ElementMaxLength);
    }
}
