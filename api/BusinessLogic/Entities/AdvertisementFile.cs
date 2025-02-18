namespace BusinessLogic.Entities;

public class AdvertisementFile : File
{
    public int AdvertisementId { get; set; }
    public Advertisement Advertisement { get; set; } = default!;
}
