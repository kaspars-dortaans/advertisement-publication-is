using BusinessLogic.Entities.Files;
using BusinessLogic.Entities.Payments;

namespace BusinessLogic.Entities;

public class Advertisement : IPaymentItemSubject
{
    public int Id { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? ValidToDate { get; set; }
    public string Title { get; set; } = default!;
    public string AdvertisementText { get; set; } = default!;
    public int ViewCount { get; set; }
    public bool IsActive { get; set; }

    public int OwnerId { get; set; }
    public User Owner { get; set; } = default!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public int? ThumbnailImageId { get; set; }
    public Files.File? ThumbnailImage { get; set; }
    public ICollection<AdvertisementAttributeValue> AttributeValues { get; set; } = default!;
    public ICollection<AdvertisementImage> Images { get; set; } = default!;
    public ICollection<User> BookmarksOwners { get; set; } = default!;
    public ICollection<AdvertisementBookmark> AdvertisementBookmarks { get; set; } = default!;
}
