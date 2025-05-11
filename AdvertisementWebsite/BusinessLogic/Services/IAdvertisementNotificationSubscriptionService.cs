using BusinessLogic.Dto.AdvertisementNotifications;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Time;
using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public interface IAdvertisementNotificationSubscriptionService : IBaseService<AdvertisementNotificationSubscription>
{
    public Task<DataTableQueryResponse<NotificationSubscriptionItem>> GetSubscriptions(DataTableQuery query, int userId);
    public IQueryable<KeyValuePair<int, string>> GetLookupByIds(IEnumerable<int> ids, int userId);
    
    /// <summary>
    /// Create new subscription draft. "Created date" and "valid to date" are not assigned.
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<int> CreateSubscription(CreateOrEditSubscription dto, int userId);
    public Task<CreateOrEditSubscription> GetSubscriptionInfo(int subscriptionId, int userId);
    public Task EditSubscription(CreateOrEditSubscription dto, int userId);
    public Task ExtendSubscriptions(IEnumerable<int> subscriptionIds, PostTimeDto time);
}
