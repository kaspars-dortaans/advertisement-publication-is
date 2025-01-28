using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public class CategoryService(Context context) : BaseService<Category>(context), ICategoryService
{
    public IQueryable<int> GetCategoryChildIds(int categoryId)
    {
        return context.GetCategoryChildIds(categoryId).Select(x => x.Id);
    }
}
