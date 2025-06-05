using AutoMapper;
using BusinessLogic.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BusinessLogic.Dto.Image;
using AdvertisementWebsite.Server.Helpers;
using BusinessLogic.Dto.Advertisement;

namespace AdvertisementWebsite.Server.Dto.Advertisement;

public class AdvertisementMapperProfile : Profile
{
    public AdvertisementMapperProfile()
    {
        CreateMap<AdvertisementListItemDto, AdvertisementListItem>()
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

        CreateMap<CreateOrEditAdvertisementRequest, CreateOrEditAdvertisementDto>()
            .ReverseMap()
                .ForMember(r => r.ImageOrder, o => o.MapFrom((dto, _, _, context) => dto.ImageOrder?.Select(imageDto => new ImageDto
                {
                    Id = imageDto.Id,
                    Hash = imageDto.Hash,
                    ImageURLs = new ImageUrl()
                    {
                        Url = FileUrlHelper.MapperGetFileUrl(context, imageDto.Id)!,
                        ThumbnailUrl = FileUrlHelper.MapperGetThumbnailUrl(context, imageDto.Id)!
                    }
                })));
    }
}
