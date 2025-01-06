using System.ComponentModel.DataAnnotations;

namespace Web.Dto.Advertisement;

public class CategoryItem
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = default!;
    [Required]
    public bool CanContainAdvertisements { get; set; }
    public int? ParentCategoryId { get; set; }
    public int? AdvertisementCount { get; set; }
}
