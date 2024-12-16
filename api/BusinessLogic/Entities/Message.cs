namespace BusinessLogic.Entities;

public class Message
{
    public int Id { get; set; }
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public DateTime SentTime { get; set; }
    public bool IsMessageRead { get; set; }
    public User FromUser { get; set; } = default!;
    public User ToUser { get; set; } = default!;
}
