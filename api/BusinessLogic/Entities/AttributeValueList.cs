namespace BusinessLogic.Entities;

public class AttributeValueList
{
    public int Id { get; set; }
    public ICollection<AttributeValueListLocaleText> LocelisedNames { get; set; } = default!;
}
