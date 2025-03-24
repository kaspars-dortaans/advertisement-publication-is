namespace BusinessLogic.Dto.Image;

public class ImageDto
{
    public int Id { get; set; }
    public ImageUrl ImageURLs { get; set; } = default!;
    public string Hash { get; set; } = default!;
}
