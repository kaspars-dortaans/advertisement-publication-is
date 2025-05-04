using BusinessLogic.Dto.Advertisement;

namespace BusinessLogic.Dto.Category;

public class CategoryInfo
{
    public string CategoryName { get; set; } = default!;
    public IEnumerable<CategoryAttributeInfo> AttributeInfo { get; set; } = default!;
    public IEnumerable<AttributeValueListItem> AttributeValueLists { get; set; } = default!;
}
