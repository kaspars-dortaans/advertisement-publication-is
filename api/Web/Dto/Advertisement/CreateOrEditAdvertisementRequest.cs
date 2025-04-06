using BusinessLogic.Constants;
using BusinessLogic.Dto.Time;
using System.ComponentModel.DataAnnotations;
using Web.Validators;

namespace Web.Dto.Advertisement;

public class CreateOrEditAdvertisementRequest
{
    public int? Id { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public int CategoryId { get; set; }

    public IEnumerable<KeyValuePair<int, string>>? AttributeValues { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public PostTimeDto PostTime { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Title { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Description { get; set; } = default!;

    [MaxFileSize(ImageConstants.MaxFileSizeInBytes)]
    [AllowedFileTypes(ImageConstants.AllowedFileTypes)]
    public IEnumerable<IFormFile>? ImagesToAdd { get; set; }

    /// <summary>
    /// Image hashes in order representing image order. Ids are not used because new uploaded images does not have an id yet.
    /// </summary>
    public IEnumerable<string>? ImageOrder { get; set; }
}
