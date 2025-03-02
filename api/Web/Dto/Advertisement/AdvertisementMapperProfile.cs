using AutoMapper;
using Web.Dto.Image;
using Web.Helpers;

namespace Web.Dto.Advertisement;

public class AdvertisementMapperProfile : Profile
{
    public AdvertisementMapperProfile()
    {
        CreateMap<BusinessLogic.Dto.Advertisement.AdvertisementListItemDto, AdvertisementListItem>()
            .ForMember(a => a.ThumbnailImageUrl, o => o
                .MapFrom((a, _, _, context) => FileUrlHelper.MapperGetThumbnailUrl(context, a.ThumbnailImageId)));

        CreateMap<BusinessLogic.Dto.Advertisement.AdvertisementDto, AdvertisementDto>()
        .ForMember(a => a.ImageURLs, o => o
            .MapFrom((a, _, _, context) => a.ImageIds
                .Select(id => new ImageUrl(){
                    Url = FileUrlHelper.MapperGetFileUrl(context, id) ?? "",
                    ThumbnailUrl = FileUrlHelper.MapperGetThumbnailUrl(context, id) ?? ""
                })));
    }
}
