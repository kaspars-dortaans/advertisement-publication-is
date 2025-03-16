namespace Web.Dto.Advertisement;

public class CategoryItem
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public bool CanContainAdvertisements { get; set; }
    public int? ParentCategoryId { get; set; }
    public int? AdvertisementCount { get; set; }
}
