using BusinessLogic.Dto.Time;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Dto.Advertisement;

public class CreateOrEditAdvertisementDto
{
    public int? Id { get; set; }

    public int CategoryId { get; set; }

    public IEnumerable<KeyValuePair<int, string>> AttributeValues { get; set; } = default!;

    public PostTimeDto PostTime { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public IEnumerable<IFormFile>? ImagesToAdd { get; set; }

    /// <summary>
    /// Image hashes in order representing image order. Ids are not used because new uploaded images does not have an id yet.
    /// </summary>
    public IEnumerable<string>? ImageOrder { get; set; }
}

