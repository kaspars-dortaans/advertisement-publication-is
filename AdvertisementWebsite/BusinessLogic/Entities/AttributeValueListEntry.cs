namespace BusinessLogic.Entities;

public class AttributeValueListEntry
{
    public int Id { get; set; }
    public int OrderIndex { get; set; }
    public int AttributeValueListId { get; set; }
    public AttributeValueList AttributeValueList { get; set; } = default!;
    public ICollection<AttributeValueListEntryLocaleText> LocalisedNames { get; set; } = default!;
}
