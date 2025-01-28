using BusinessLogic.Enums;

namespace Web.Dto.Advertisement;

public class CategoryAttributeInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public bool Sortable { get; set; }
    public bool Searchable { get; set; }
    public int Order { get; set; }
    public int? ValueListId { get; set; }
    public string? IconUrl { get; set; }
    public ValueTypes AttributeValueType { get; set; } = default!;
    public FilterType AttributeFilterType { get; set; } = default;
}
