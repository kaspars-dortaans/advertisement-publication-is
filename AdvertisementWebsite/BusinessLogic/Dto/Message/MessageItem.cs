namespace BusinessLogic.Dto.Message;

public class MessageItem
{
    public int Id { get; set; }
    public int FromUserId { get; set; }
    public string Text { get; set; } = default!;
    public DateTime SentTime { get; set; }
    public bool IsMessageRead { get; set; }
    public IEnumerable<MessageAttachmentItem> Attachments { get; set; } = default!;
}
