using BusinessLogic.Entities.LocaleTexts;

namespace BusinessLogic.Entities;

public class AttributeValueList
{
    public int Id { get; set; }
    public ICollection<AttributeValueListEntry> ListEntries { get; set; } = default!;
    public ICollection<AttributeValueListLocaleText> LocalisedNames { get; set; } = default!;
}
