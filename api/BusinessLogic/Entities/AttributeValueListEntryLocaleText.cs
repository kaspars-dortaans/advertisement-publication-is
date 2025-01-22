namespace BusinessLogic.Entities;

public class AttributeValueListEntryLocaleText : LocaleText
{
    public int AttributeValueListEntryId { get; set; }
    public AttributeValueListEntry ListEntry { get; set; } = default!;
}
