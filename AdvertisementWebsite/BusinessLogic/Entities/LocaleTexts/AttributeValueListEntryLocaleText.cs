namespace BusinessLogic.Entities.LocaleTexts;

public class AttributeValueListEntryLocaleText : LocaleText
{
    public int AttributeValueListEntryId { get; set; }
    public AttributeValueListEntry ListEntry { get; set; } = default!;
}
