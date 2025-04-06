using BusinessLogic.Constants;
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
    public int PostDayCount { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Title { get; set; } = default!;

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public string Description { get; set; } = default!;

    /// <summary>
    /// Not id in order to be able assign new images as thumbnail
    /// </summary>
    public string? ThumbnailImageHash { get; set; }

    [MaxFileSize(ImageConstants.MaxFileSizeInBytes)]
    [AllowedFileTypes(ImageConstants.AllowedFileTypes)]
    public IEnumerable<IFormFile>? ImagesToAdd { get; set; }
    public IEnumerable<int>? ImageIdsToDelete { get; set; }
}
