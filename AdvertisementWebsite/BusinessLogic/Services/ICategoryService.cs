using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public interface ICategoryService : IBaseService<Category>
{
    public IQueryable<int> GetCategoryChildIds(int categoryId);
    public IQueryable<int> GetParentCategoryIds(int categoryId);
    public IQueryable<CategoryAttribute> GetCategoryAndParentAttributes(int categoryId);
}
