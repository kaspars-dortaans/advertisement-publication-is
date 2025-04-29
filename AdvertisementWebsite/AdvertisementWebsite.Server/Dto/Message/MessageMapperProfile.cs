using AutoMapper;
using BusinessLogic.Dto.Message;
using AdvertisementWebsite.Server.Helpers;

namespace AdvertisementWebsite.Server.Dto.Message;

public class MessageMapperProfile : Profile
{
    public MessageMapperProfile() {
        CreateMap<ChatListItem, ChatListItemDto>()
            .ForMember(c => c.ThumbnailImageUrl, o => o.MapFrom((item, _, _, context) => FileUrlHelper.MapperGetThumbnailUrl(context, item.ThumbnailImageId)));

        CreateMap<MessageItem, MessageItemDto>();

        CreateMap<MessageAttachmentItem, MessageAttachmentItemDto>()
            .ForMember(m => m.Url, o => o.MapFrom((m, _, _, context) => FileUrlHelper.MapperGetFileUrl(context, m.Id)));
    }
}
