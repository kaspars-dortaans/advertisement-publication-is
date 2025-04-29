using BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public class CategoryService(Context dbContext) : BaseService<Category>(dbContext), ICategoryService
{
    

    public IQueryable<int> GetCategoryChildIds(int categoryId)
    {
        return DbContext.GetCategoryChildIds(categoryId).Select(x => x.Id);
    }

    public IQueryable<int> GetParentCategoryIds(int categoryId)
    {
        return DbContext.GetCategoryParentIds(categoryId).Select(x => x.Id);
    }

    public IQueryable<CategoryAttribute> GetCategoryAndParentAttributes(int categoryId)
    {
        var parentCategoryIds = GetParentCategoryIds(categoryId);
        return DbContext.CategoryAttributes
            .Where(ca => ca.CategoryId == categoryId || parentCategoryIds.Contains(ca.CategoryId));
    }
}
