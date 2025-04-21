using BusinessLogic.Dto.AdvertisementNotifications;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Time;
using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public interface IAdvertisementNotificationSubscriptionService : IBaseService<AdvertisementNotificationSubscription>
{
    public Task<DataTableQueryResponse<NotificationSubscriptionItem>> GetSubscriptions(DataTableQuery query, int userId);
    public IQueryable<KeyValuePair<int, string>> GetLookupByIds(IEnumerable<int> ids, int userId);
    public Task CreateSubscription(CreateOrEditSubscription dto, int userId);
    public Task<CreateOrEditSubscription> GetSubscriptionInfo(int subscriptionId, int userId);
    public Task EditSubscription(CreateOrEditSubscription dto, int userId);
    public Task ExtendSubscriptions(IEnumerable<int> subscriptionIds, PostTimeDto time, int userId);
}
