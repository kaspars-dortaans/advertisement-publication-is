namespace api.Entities;

public class Image
{
    public int Id { get; set; }
    public string Path { get; set; } = default!;
    public int OwnerUserId { get; set; }
    public int Advertisementid { get; set; }
    public User OwnerUser { get; set; } = default!;
    public Advertisement Advertisement { get; set; } = default!;
}
