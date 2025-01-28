namespace BusinessLogic.Entities;

public class UserFile : File
{
    public int OwnerUserId { get; set; }
    public User OwnerUser { get; set; } = default!;
}
