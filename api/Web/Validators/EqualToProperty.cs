using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace Web.Validators;

public class EqualToProperty : ValidationAttribute
{
    private readonly string comparablePropertyName;

    public EqualToProperty(string propertyName) : base()
    {
        comparablePropertyName = propertyName;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        var comparableProperty = ctx.ObjectType
            ?.GetProperty(comparablePropertyName);

        if (comparableProperty == null)
        {
            return new ValidationResult(CustomErrorCodes.PropertyWasNotFound, new List<string> { comparablePropertyName });
        }

        var comparableValue = comparableProperty.GetValue(ctx.ObjectInstance);
        if ((value is not null && value.Equals(comparableValue)) || (value == comparableValue))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(CustomErrorCodes.ComparablePropertyDidNotMatch, new List<string> { ctx.MemberName ?? "", comparablePropertyName });
    }
}
