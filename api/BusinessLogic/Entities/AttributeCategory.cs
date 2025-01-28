using BusinessLogic.Enums;

namespace BusinessLogic.Entities;

public class CategoryAttribute
{
    public int AttributeId { get; set; }
    public int CategoryId { get; set; }
    public int AttributeOrder { get; set; }

    public Category Category { get; set; } = default!;
    public Attribute Attribute { get; set; } = default!;
}
