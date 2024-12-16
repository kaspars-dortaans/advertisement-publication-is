namespace BusinessLogic.Entities;

public class File
{
    public int Id { get; set; }
    public string Path { get; set; } = default!;
    public int OwnerUserId { get; set; }
    public User OwnerUser { get; set; } = default!;
}