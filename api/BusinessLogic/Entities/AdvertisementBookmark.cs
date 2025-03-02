namespace BusinessLogic.Entities;

public class AdvertisementBookmark
{
    public int BookmarkedAdvertisementId { get; set; }
    public int BookmarkOwnerId { get; set; }

    public Advertisement BookmarkedAdvertisement { get; set; } = default!;
    public User BookmarkOwner { get; set; } = default!;
}
