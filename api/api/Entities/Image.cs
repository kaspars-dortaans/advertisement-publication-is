namespace api.Entities;

public class Image : File
{
    public int AdvertisementId { get; set; }
    public Advertisement Advertisement { get; set; } = default!;
}
