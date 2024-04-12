using AutoMapper;

namespace api.Dto.User;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<Entities.User, UserListItem>();
    }
}
