using AutoMapper;
using BusinessLogic.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
                    .Select(id => new ImageUrl()
                    {
                        Url = FileUrlHelper.MapperGetFileUrl(context, id) ?? "",
                        ThumbnailUrl = FileUrlHelper.MapperGetThumbnailUrl(context, id) ?? ""
                    })));

        CreateMap<ReportAdvertisementRequest, RuleViolationReport>()
            .ForMember(report => report.ReportDate, o => o.MapFrom(request => DateTime.UtcNow))
            .ForMember(report => report.ReporterId, o => o.MapFrom((request, _, _, context) =>
                (context.Items[nameof(ControllerBase.User)] as ClaimsPrincipal)?.GetUserId()));
    }
}
