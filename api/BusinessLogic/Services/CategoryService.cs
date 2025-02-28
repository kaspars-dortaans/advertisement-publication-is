using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public class CategoryService : BaseService<Category>, ICategoryService
{
    public CategoryService(Context dbContext) : base(dbContext)
    {

    }
    public IQueryable<int> GetCategoryChildIds(int categoryId)
    {
        return DbContext.GetCategoryChildIds(categoryId).Select(x => x.Id);
    }
}
