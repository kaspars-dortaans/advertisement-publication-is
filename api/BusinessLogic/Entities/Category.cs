namespace BusinessLogic.Entities;

public class Category
{
    public int Id { get; set; }
    public bool CanContainAdvertisements { get; set; }
    public int? AdvertisementCount { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> ChildCategories { get; set; } = default!;
    public ICollection<CategoryNameLocaleText> LocalisedNames { get; set; } = default!;
    public ICollection<Attribute> Attributes { get; set; } = default!;
}
