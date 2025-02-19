using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Web.Controllers;

namespace Web.Dto.Advertisement;

public class AdvertisementMapperProfile : Profile
{
    public AdvertisementMapperProfile()
    {
        CreateMap<BusinessLogic.Dto.Advertisement.AdvertisementListItemDto, AdvertisementListItem>()
            .ForMember(a => a.ThumbnailImageUrl, o => o
                .MapFrom((a, _, _, context) => GetFileUrl(context, a.ThumbnailImageId)));
    }

    private static string? GetFileUrl(ResolutionContext context, int? id)
    {
        if (id == null)
        {
            return null;
        }
        return (context.Items[nameof(ControllerBase.Url)] as IUrlHelper)!.Link(nameof(FileController.GetFile), new { id });
    }
}
