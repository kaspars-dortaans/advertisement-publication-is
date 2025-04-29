using AutoMapper;
using BusinessLogic.Dto.AdvertisementNotifications;

namespace AdvertisementWebsite.Server.Dto.AdvertisementNotification;

public class AdvertisementNotificationMapperProfile : Profile
{
    public AdvertisementNotificationMapperProfile()
    {
        CreateMap<CreateOrEditSubscription, CreateOrEditNotificationSubscriptionRequest>()
            .ReverseMap();
    }
}
