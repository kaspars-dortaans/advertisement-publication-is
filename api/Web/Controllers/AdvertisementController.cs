using AutoMapper;
using BusinessLogic.Dto.Advertisement;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;
using BusinessLogic.Helpers;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Dto.Advertisement;

namespace Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class AdvertisementController(
    ILogger<AdvertisementController> logger,
    IMapper mapper,
    IBaseService<Category> categoryService,
    IAdvertisementService advertisementService) : ControllerBase
{
    private readonly ILogger<AdvertisementController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IBaseService<Category> _categoryService = categoryService;
    private readonly IAdvertisementService _advertisementService = advertisementService;

    [AllowAnonymous]
    [HttpPost]
    public Task<DataTableQueryResponse<AdvertisementListItem>> GetAdvertisements(AdvertismentQuery request)
    {
        return _advertisementService.GetActiveAdvertisementsByCategory(request);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IEnumerable<CategoryItem>> GetCategories(string locale)
    {
        var normalizedLocale = locale.ToUpper();
        return await _categoryService
            .GetAll()
            .Select(c => new CategoryItem()
            {
                Id = c.Id,
                ParentCategoryId = c.ParentCategoryId,
                AdvertisementCount = c.AdvertisementCount,
                CanContainAdvertisements = c.CanContainAdvertisements,
                Name = c.LocalisedNames.Localise(normalizedLocale),
            })
            .ToListAsync();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<CategoryInfo> GetCategoryInfo(int categoryId, string locale)
    {
        var normalizedLocale = locale.ToUpper();
        var result = await _categoryService
            .Where(c => c.Id == categoryId)
            .Select(c => new CategoryInfo()
            {
                CategoryName = c.LocalisedNames.Localise(normalizedLocale),
                AttributeInfo = c.CategoryAttributes.Select(ca => new CategoryAttributeInfo()
                {
                    Id = ca.Attribute.Id,
                    Name = ca.Attribute.AttributeNameLocales.Localise(normalizedLocale),
                    Searchable = ca.Attribute.Searchable,
                    Sortable = ca.Attribute.Sortable,
                    Order = ca.AttributeOrder,
                    ValueListId = ca.Attribute.AttributeValueListId,
                    AttributeFilterType = ca.Attribute.FilterType,
                    AttributeValueType = ca.Attribute.ValueType,
                    IconUrl = ca.Attribute.Icon != null ? ca.Attribute.Icon.Path : null
                }),
                AttributeValueLists = c.Attributes
                    .Where(a => a.AttributeValueList != null)
                    .Select(a => new AttributeValueListItem()
                    {
                        Id = a.AttributeValueList!.Id,
                        Name = a.AttributeValueList!.LocalisedNames.Localise(normalizedLocale),
                        Entries = a.AttributeValueList!.ListEntries.Select(e =>  new AttributeValueListEntryItem()
                        {
                            Id = e.Id,
                            Name = e.LocalisedNames.Localise(normalizedLocale),
                            OrderIndex = e.OrderIndex
                        }),
                    })
            })
            .FirstAsync();

        return result;
    }
}