namespace BusinessLogic.Entities;

public class Chat
{
    public int Id { get; set; }
    public int? AdvertisementId { get; set; }   
    public Advertisement? Advertisement { get; set; }
    public ICollection<Message> ChatMessages { get; set; } = default!;
    public ICollection<User> Users { get; set; } = default!;
    public ICollection<ChatUser> ChatUsers { get; set; } = default!;
}
