using BusinessLogic.Dto.Advertisement;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public interface IAdvertisementService : IBaseService<Advertisement>
{
    public Task<DataTableQueryResponse<AdvertisementListItem>> GetActiveAdvertisementsByCategory(AdvertismentQuery request);
}
