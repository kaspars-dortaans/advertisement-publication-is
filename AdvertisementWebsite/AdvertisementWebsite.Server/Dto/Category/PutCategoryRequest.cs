using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Category;

public class PutCategoryRequest
{
    public int? Id { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    [MinLength(1, ErrorMessage = CustomErrorCodes.MissingRequired)]
    public IEnumerable<KeyValuePair<string, string>> LocalizedNames{ get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public bool CanContainAdvertisements { get; set; }
    
    public int? ParentCategoryId { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public IEnumerable<KeyValuePair<int, string>> CategoryAttributeOrder { get; set; } = default!;
}

