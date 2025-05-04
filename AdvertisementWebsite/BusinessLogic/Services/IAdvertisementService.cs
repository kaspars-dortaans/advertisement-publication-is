using BusinessLogic.Dto.Advertisement;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Time;
using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public interface IAdvertisementService : IBaseService<Advertisement>
{
    public Task<DataTableQueryResponse<AdvertisementListItemDto>> GetActiveAdvertisementsByCategory(AdvertisementQuery request);
    public Task<DataTableQueryResponse<AdvertisementListItemDto>> GetBookmarkedAdvertisements(AdvertisementQuery request, int currentUserId);
    public Task<AdvertisementDto> FindOwnedOrActiveAdvertisement(int advertisementId, int? userId);
    public Task BookmarkAdvertisement(int advertisementId, int userId, bool addBookmark);
    public Task<DataTableQueryResponse<AdvertisementInfo>> GetAdvertisementInfo(DataTableQuery query, int? userId);
    public Task RemoveAdvertisements(IEnumerable<int> advertisementIds, int userId);
    
    /// <summary>
    /// Creates new advertisement draft. "Created date" and "valid to date" are not assigned.
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<int> CreateAdvertisement(CreateOrEditAdvertisementDto dto, int userId);
    public Task UpdateAdvertisement(CreateOrEditAdvertisementDto dto, int userId);
    public Task<CreateOrEditAdvertisementDto> GetAdvertisementFormInfo(int advertisementId, int userId);
    public Task ExtendAdvertisement(int userId, IEnumerable<int> advertisementId, PostTimeDto extendTime);
}
