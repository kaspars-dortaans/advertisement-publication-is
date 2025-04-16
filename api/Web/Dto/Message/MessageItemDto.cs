namespace Web.Dto.Message;

public class MessageItemDto
{
    public int Id { get; set; }
    public int FromUserId { get; set; }
    public string Text { get; set; } = default!;
    public DateTime SentTime { get; set; }
    public bool IsMessageRead { get; set; }
    public IEnumerable<MessageAttachmentItemDto> Attachments { get; set; } = default!;
}
