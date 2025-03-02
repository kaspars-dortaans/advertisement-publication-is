namespace BusinessLogic.Entities.Files;

public class UserImage : Image
{
    public int OwnerUserId { get; set; }
    public User OwnerUser { get; set; } = default!;
}
