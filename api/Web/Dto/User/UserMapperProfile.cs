using AutoMapper;

namespace Web.Dto.User;

public class UserMapperProfile : Profile
{
    public UserMapperProfile() {
        CreateMap<RegisterDto, BusinessLogic.Entities.User>();
    }
}
