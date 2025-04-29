using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Validators;

public class EqualToProperty(string propertyName) : ValidationAttribute()
{
    private readonly string comparablePropertyName = propertyName;

    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        var comparableProperty = ctx.ObjectType
            ?.GetProperty(comparablePropertyName);

        if (comparableProperty == null)
        {
            return new ValidationResult(CustomErrorCodes.PropertyWasNotFound, [comparablePropertyName]);
        }

        var comparableValue = comparableProperty.GetValue(ctx.ObjectInstance);
        if ((value is not null && value.Equals(comparableValue)) || (value == comparableValue))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(CustomErrorCodes.ComparablePropertyDidNotMatch, [ctx.MemberName ?? "", comparablePropertyName]);
    }
}
