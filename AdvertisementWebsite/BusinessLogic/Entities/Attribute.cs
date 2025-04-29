using BusinessLogic.Entities.Files;
using BusinessLogic.Enums;

namespace BusinessLogic.Entities;

public class Attribute
{
    public int Id { get; set; }
    public ValueTypes ValueType { get; set; }
    public FilterType FilterType { get; set; }
    public string? ValueValidationRegex { get; set; }
    public int? AttributeValueListId { get; set; }
    public bool Sortable { get; set; }
    public bool Searchable { get; set; }
    public int? IconId { get; set; }

    public SystemImage? Icon { get; set; }
    public AttributeValueList? AttributeValueList { get; set; }
    public ICollection<Category> UsedInCategories { get; set; } = default!;
    public ICollection<CategoryAttribute> CategoryAttributes { get; set; } = default!;
    public ICollection<AttributeNameLocaleText> AttributeNameLocales { get; set; } = default!;
}
