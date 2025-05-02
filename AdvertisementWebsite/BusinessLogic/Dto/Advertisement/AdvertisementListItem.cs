namespace BusinessLogic.Dto.Advertisement;
public class AdvertisementListItem
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
    public string Title { get; set; } = default!;
    public string AdvertisementText { get; set; } = default!;
    public string? ThumbnailImageUrl { get; set; }
    public IEnumerable<AttributeValueItem> AttributeValues { get; set; } = default!;
}