namespace Web.Dto.Message;

public class ChatListItemDto
{
    public int Id { get; set; }
    public int? AdvertisementId { get; set; }
    public int? AdvertisementOwnerId { get; set; }
    public string Title { get; set; } = default!;
    public string LastMessage { get; set; } = default!;
    public int UnreadMessageCount { get; set; }
    public string? ThumbnailImageUrl { get; set; }
}
