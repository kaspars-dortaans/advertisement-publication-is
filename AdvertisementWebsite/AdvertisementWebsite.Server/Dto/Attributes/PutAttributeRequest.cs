using BusinessLogic.Constants;
using BusinessLogic.Enums;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Attributes;

public class PutAttributeRequest
{
    public int? Id { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public ValueTypes ValueType { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public FilterType FilterType { get; set; }
    public string? ValueValidationRegex { get; set; }
    public int? AttributeValueListId { get; set; }
    public string? AttributeValueListName { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public bool Sortable { get; set; }
    
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public bool Searchable { get; set; }
    public string? IconName { get; set; }
    
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public bool ShowOnListItem { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public IEnumerable<KeyValuePair<string, string>?> LocalizedNames{ get; set; } = default!;
}
