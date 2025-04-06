using BusinessLogic.Enums;

namespace Web.Dto.Advertisement;

public class AttributeFormInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int Order { get; set; }
    public string? ValueValidationRegex { get; set; }
    public int? ValueListId { get; set; }
    public string? IconUrl { get; set; }
    public ValueTypes AttributeValueType { get; set; } = default!;
}
