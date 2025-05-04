using BusinessLogic.Dto.Advertisement;

namespace BusinessLogic.Dto.Category;

public class CategoryFormInfo
{
    public IEnumerable<AttributeFormInfo> AttributeInfo { get; set; } = default!;
    public IEnumerable<AttributeValueListItem> AttributeValueLists { get; set; } = default!;
}
