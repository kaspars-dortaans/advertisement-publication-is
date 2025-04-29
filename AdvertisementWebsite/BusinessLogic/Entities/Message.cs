using BusinessLogic.Entities.Files;

namespace BusinessLogic.Entities;

public class Message
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public int FromUserId { get; set; }
    public string Text { get; set; } = default!;
    public DateTime SentTime { get; set; }
    public bool IsMessageRead { get; set; }
    public Chat Chat { get; set; } = default!;
    public User FromUser { get; set; } = default!;
    public IEnumerable<MessageAttachment> Attachments { get; set; } = default!;
}
