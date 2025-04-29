using AutoMapper;
using BusinessLogic.Dto.Image;
using AdvertisementWebsite.Server.Helpers;

namespace AdvertisementWebsite.Server.Dto.User;

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
            .ForMember(dto => dto.ProfileImage, o => o
                .MapFrom((u, _, _, context) => u.ProfileImageFile is not null
                ? new ImageDto()
                {
                    Id = u.ProfileImageFile.Id,
                    Hash = u.ProfileImageFile.Hash,
                    ImageURLs = new ImageUrl()
                    {
                        Url = FileUrlHelper.MapperGetFileUrl(context, u.ProfileImageFileId)!,
                        ThumbnailUrl = FileUrlHelper.MapperGetThumbnailUrl(context, u.ProfileImageFileId)!
                    }
                }
                : null));

        CreateMap<EditUserInfo, BusinessLogic.Entities.User>();
    }
}
