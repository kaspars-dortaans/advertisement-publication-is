using BusinessLogic.Dto.Category;
using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public interface ICategoryService : IBaseService<Category>
{
    public IQueryable<int> GetCategoryChildIds(int categoryId);
    public IQueryable<int> GetParentCategoryIds(int categoryId);
    public IQueryable<CategoryAttribute> GetCategoryAndParentAttributes(int categoryId);
    public IQueryable<int> GetCategoryListFromAdvertisementIds(IEnumerable<int> ids);
    public Task<CategoryInfo> GetCategoryInfo(int categoryId);
    public Task<CategoryAttributeListData> GetCategoryFormInfo(int categoryId);
    public Task UpdateCategory(Category category);
}
