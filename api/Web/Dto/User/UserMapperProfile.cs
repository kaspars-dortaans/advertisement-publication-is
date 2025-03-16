using AutoMapper;
using BusinessLogic.Dto.Image;
using Web.Helpers;

namespace Web.Dto.User;

public class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        CreateMap<RegisterDto, BusinessLogic.Entities.User>();
        CreateMap<BusinessLogic.Entities.User, UserListItem>();
        CreateMap<BusinessLogic.Entities.User, PublicUserInfoDto>()
            .ForMember(dto => dto.ProfileImageUrl, o => o
                .MapFrom((u, _, _, context) => FileUrlHelper.MapperGetFileUrl(context, u.ProfileImageFileId)))
            .ForMember(dto => dto.Email, o => o.MapFrom(u => u.IsEmailPublic ? u.Email : null))
            .ForMember(dto => dto.PhoneNumber, o => o.MapFrom(u => u.IsPhoneNumberPublic ? u.PhoneNumber : null));

        CreateMap<BusinessLogic.Entities.User, UserInfo>()
            .ForMember(dto => dto.ProfileImageUrl, o => o
                .MapFrom((u, _, _, context) => new ImageUrl()
                    {
                        Url = FileUrlHelper.MapperGetFileUrl(context, u.ProfileImageFileId)!,
                        ThumbnailUrl = FileUrlHelper.MapperGetThumbnailUrl(context, u.ProfileImageFileId)!
                    }));

        CreateMap<EditUserInfo, BusinessLogic.Entities.User>();
    }
}
