using AutoMapper;

namespace Web.Dto.User;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<BusinessLogic.Entities.User, UserListItem>();
    }
}
