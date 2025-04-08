using BusinessLogic.Constants;
using BusinessLogic.Dto.Image;
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

    public PostTimeDto? PostTime { get; set; }

    /// <summary>
    /// Displayed on edit, readonly
    /// </summary>
    public DateTime? ValidTo { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Title { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Description { get; set; } = default!;

    [MaxFileSize(ImageConstants.MaxFileSizeInBytes)]
    [AllowedFileTypes(ImageConstants.AllowedFileTypes)]
    public IEnumerable<IFormFile>? ImagesToAdd { get; set; }

    /// <summary>
    /// Image DTO's order representing image order.
    /// </summary>
    public IEnumerable<ImageDto>? ImageOrder { get; set; }
}
