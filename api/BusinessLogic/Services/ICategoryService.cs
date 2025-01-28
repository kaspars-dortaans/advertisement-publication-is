using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public interface ICategoryService : IBaseService<Category>
{
    public IQueryable<int> GetCategoryChildIds(int categoryId);
}
