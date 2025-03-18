using BusinessLogic.Dto.Advertisement;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public interface IAdvertisementService : IBaseService<Advertisement>
{
    public Task<DataTableQueryResponse<AdvertisementListItemDto>> GetActiveAdvertisementsByCategory(AdvertisementQuery request);
    public Task<DataTableQueryResponse<AdvertisementListItemDto>> GetBookmarkedAdvertisements(AdvertisementQuery request, int currentUserId);
    public Task<AdvertisementDto> FindOwnedOrActiveAdvertisement(int advertisementId, int? userId);
    public Task BookmarkAdvertisement(int advertisementId, int userId, bool addBookmark);
    public IQueryable<int> GetCategoryListFromAdvertisementIds(IEnumerable<int> ids);
    public Task<DataTableQueryResponse<AdvertisementInfo>> GetAdvertisementInfo(DataTableQuery query, int? userId);
}
