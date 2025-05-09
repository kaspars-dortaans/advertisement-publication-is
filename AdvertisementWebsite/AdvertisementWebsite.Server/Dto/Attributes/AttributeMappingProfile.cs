using AutoMapper;
using BusinessLogic.Entities.LocaleTexts;

namespace AdvertisementWebsite.Server.Dto.Attributes;

public class AttributeMappingProfile : Profile
{
    public AttributeMappingProfile()
    {
        CreateMap<PutAttributeRequest, BusinessLogic.Entities.Attribute>()
            .ForMember(a => a.AttributeNameLocales, opts => opts.MapFrom(r => r.LocalizedNames.Where(ln => ln != null).Select(n => new AttributeNameLocaleText
            {
                AttributeId = r.Id ?? default,
                Locale = n.Value.Key,
                Text = n.Value.Value ?? string.Empty
            })));
    }
}
