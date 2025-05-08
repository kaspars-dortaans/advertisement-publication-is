using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Attributes;

public class AttributeValueListEntryDto
{
    public int? Id { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public int OrderIndex { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public IEnumerable<KeyValuePair<string, string>> LocalizedNames { get; set; } = default!;
}
