namespace api.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public int? ParentCategoryId { get; set; }
        public string Name { get; set; }
        public bool ContainsAdvertisements { get; set; }
        public Category? ParentCategory { get; set; }
    }
}
