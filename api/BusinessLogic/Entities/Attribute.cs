namespace BusinessLogic.Entities;

public class Attribute
{
    public int Id { get; set; }
    public Enums.ValueTypes ValueType { get; set; }
    public string? ValueValidationRegex { get; set; }
    public int? AttributeValueListId { get; set; }

    public AttributeValueList? AttributeValueList{ get; set; }
    public ICollection<Category> UsedInCategories { get; set; } = default!;
    public ICollection<AttributeNameLocaleText> AttributeNameLocales { get; set; } = default!;
}
