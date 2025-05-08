using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Attributes;

public class PutAttributeValueListRequest
{
    public int? Id { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public IEnumerable<KeyValuePair<string, string>> LocalizedNames { get; set; } = default!;
    public IEnumerable<AttributeValueListEntryDto> Entries { get; set; } = default!;
}
