namespace BusinessLogic.Dto.Advertisement;

public class AttributeValueListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public IEnumerable<AttributeValueListEntryItem> Entries { get; set; } = default!;
}
