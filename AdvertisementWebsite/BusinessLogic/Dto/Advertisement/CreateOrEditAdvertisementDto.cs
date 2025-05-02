using BusinessLogic.Dto.Image;
using BusinessLogic.Dto.Time;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Dto.Advertisement;

public class CreateOrEditAdvertisementDto
{
    public int? Id { get; set; }

    public int CategoryId { get; set; }

    public IEnumerable<KeyValuePair<int, string>> AttributeValues { get; set; } = default!;

    public PostTimeDto PostTime { get; set; } = default!;

    /// <summary>
    /// Displayed on edit, readonly
    /// </summary>
    public DateTime? ValidToDate { get; set; }

    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;
    
    public IEnumerable<IFormFile>? ImagesToAdd { get; set; }

    /// <summary>
    /// Image DTO's order representing image order.
    /// </summary>
    public IEnumerable<ImageDto>? ImageOrder { get; set; }
}

