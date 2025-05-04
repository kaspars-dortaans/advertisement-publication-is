using BusinessLogic.Enums;

namespace BusinessLogic.Dto.Advertisement;

public class AttributeFormInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int Order { get; set; }
    public string? ValueValidationRegex { get; set; }
    public int? ValueListId { get; set; }
    public string? IconName { get; set; }
    public ValueTypes AttributeValueType { get; set; } = default!;
}
