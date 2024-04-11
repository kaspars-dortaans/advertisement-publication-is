namespace api.Entities;

public class Advertisement
{
    public int Id { get; set; }
    public DateTime PostedDate { get; set; }
    public DateTime ValidToDate { get; set; }
    public string Title { get; set; } = default!;
    public string AdvertisementText { get; set; } = default!;
    public int ViewCount { get; set; }

    public int OwnerId { get; set; }
    public int CategoryId { get; set; }
    public int? ThumbnailImageId { get; set; }
    public User Owner { get; set; } = default!;
    public Category Category { get; set; } = default!;
    public Image? ThumbnailImage { get; set; }
}
