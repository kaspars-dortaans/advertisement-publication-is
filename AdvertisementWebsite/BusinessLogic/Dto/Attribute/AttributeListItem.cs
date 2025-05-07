using BusinessLogic.Enums;

namespace BusinessLogic.Dto.Attribute;

public class AttributeListItem
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public ValueTypes ValueType { get; set; }
    public FilterType FilterType { get; set; }
    public string? AttributeValueListName { get; set; }
    public bool Sortable { get; set; }
    public bool Searchable { get; set; }
    public bool ShowOnListItem { get; set; }
    public string? IconName { get; set; }
}
