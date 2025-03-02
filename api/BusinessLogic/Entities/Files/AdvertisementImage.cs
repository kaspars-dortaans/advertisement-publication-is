namespace BusinessLogic.Entities.Files;

public class AdvertisementImage : Image
{
    public int AdvertisementId { get; set; }
    public Advertisement Advertisement { get; set; } = default!;
}
