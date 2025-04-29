namespace BusinessLogic.Entities;

public class ChatUser
{
    public int ChatId { get; set; }
    public int UserId { get; set; }

    public Chat Chat { get; set; } = default!;
    public User User { get; set; } = default!;
}
