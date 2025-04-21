using AutoMapper;
using BusinessLogic.Dto.AdvertisementNotifications;

namespace Web.Dto.AdvertisementNotification;

public class AdvertisementNotificationMapperProfile : Profile
{
    public AdvertisementNotificationMapperProfile()
    {
        CreateMap<CreateOrEditSubscription, CreateOrEditNotificationSubscriptionRequest>()
            .ReverseMap();
    }
}
