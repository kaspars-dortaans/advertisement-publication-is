namespace Web.Dto.Advertisement;

public class CategoryInfo
{
    public string CategoryName { get; set; } = default!;
    public IEnumerable<CategoryAttributeInfo> AttributeInfo { get; set; } = default!;
    public IEnumerable<AttributeValueListItem> AttributeValueLists { get; set; } = default!;
}
