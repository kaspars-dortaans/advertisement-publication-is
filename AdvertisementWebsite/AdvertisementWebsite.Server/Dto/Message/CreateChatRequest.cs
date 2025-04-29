namespace AdvertisementWebsite.Server.Dto.Message;

public class CreateChatRequest
{
    public int UserId { get; set; }
    public int? AdvertisementId { get; set; }
    public SendMessageRequest WithMessage { get; set; } = default!;
}
