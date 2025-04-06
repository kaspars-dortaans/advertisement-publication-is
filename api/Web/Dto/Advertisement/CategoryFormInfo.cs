namespace Web.Dto.Advertisement;

public class CategoryFormInfo
{
    public IEnumerable<AttributeFormInfo> AttributeInfo { get; set; } = default!;
    public IEnumerable<AttributeValueListItem> AttributeValueLists { get; set; } = default!;
}
