using AutoMapper;
using BusinessLogic.Entities;
using BusinessLogic.Entities.LocaleTexts;

namespace AdvertisementWebsite.Server.Dto.Category;

public class CategoryMapperProfile : Profile
{
    public CategoryMapperProfile() {
        CreateMap<PutCategoryRequest, BusinessLogic.Entities.Category>()
            .ForMember(c => c.LocalisedNames, opts => opts.MapFrom(r => r.LocalizedNames.Select(p => new CategoryNameLocaleText
            {
                CategoryId = r.Id ?? default,
                Locale = p.Key,
                Text = p.Value
            })))
            .ForMember(c => c.CategoryAttributes, opts => opts.MapFrom(r => r.CategoryAttributeOrder.Select((cao, i)=> new CategoryAttribute
            {
                CategoryId = r.Id ?? default,
                AttributeId = cao.Key,
                AttributeOrder = i
            })));
    }
}
