using BusinessLogic.Constants;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Dto.Advertisement;

public class CreateOrEditAdvertisementDto
{
    public int? Id { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public IEnumerable<KeyValuePair<int, string>> AttributeValues { get; set; } = default!;

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

    public IEnumerable<IFormFile>? ImagesToAdd { get; set; }
    public IEnumerable<int>? ImageIdsToDelete { get; set; }
}

