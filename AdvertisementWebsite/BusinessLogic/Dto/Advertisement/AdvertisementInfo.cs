namespace BusinessLogic.Dto.Advertisement;

public class AdvertisementInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime ValidTo { get; set; }
    public DateTime CreatedAt { get; set; }
}
