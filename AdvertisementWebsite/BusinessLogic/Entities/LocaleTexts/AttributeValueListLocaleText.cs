namespace BusinessLogic.Entities.LocaleTexts;

public class AttributeValueListLocaleText : LocaleText
{
    public int AttributeValueListId { get; set; }
    public AttributeValueList ValueList { get; set; } = default!;
}
