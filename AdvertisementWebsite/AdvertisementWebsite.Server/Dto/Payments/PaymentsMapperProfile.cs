using AutoMapper;
using BusinessLogic.Dto.Payment;

namespace AdvertisementWebsite.Server.Dto.Payments;

public class PaymentsMapperProfile : Profile
{
    public PaymentsMapperProfile()
    {
        CreateMap<NewPaymentItem, PaymentItemDto>();
    }
}
