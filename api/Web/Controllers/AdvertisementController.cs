using AutoMapper;
using BusinessLogic.Dto.Advertisement;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.CookieSettings;
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
    IAdvertisementService advertisementService,
    CookieSettingsHelper cookieSettingsHelper) : ControllerBase
{
    private readonly ILogger<AdvertisementController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IBaseService<Category> _categoryService = categoryService;
    private readonly IAdvertisementService _advertisementService = advertisementService;
    private readonly CookieSettingsHelper _cookieSettingsHelper = cookieSettingsHelper;

    [AllowAnonymous]
    [HttpPost]
    public async Task<DataTableQueryResponse<AdvertisementListItem>> GetAdvertisements(AdvertisementQuery request)
    {
        var result = await _advertisementService.GetActiveAdvertisementsByCategory(request);
        return result.MapDataTableResponse<AdvertisementListItemDto, AdvertisementListItem>(_mapper, opts => opts.Items[nameof(Url)] = Url);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IEnumerable<CategoryItem>> GetCategories()
    {
        return await _categoryService
            .GetAll()
            .Select(c => new CategoryItem()
            {
                Id = c.Id,
                ParentCategoryId = c.ParentCategoryId,
                AdvertisementCount = c.AdvertisementCount,
                CanContainAdvertisements = c.CanContainAdvertisements,
                Name = c.LocalisedNames.Localise(_cookieSettingsHelper.Settings.NormalizedLocale),
            })
            .ToListAsync();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<CategoryInfo> GetCategoryInfo(int categoryId)
    {
        var locale = _cookieSettingsHelper.Settings.NormalizedLocale;
        var result = await _categoryService
            .Where(c => c.Id == categoryId)
            .Select(c => new CategoryInfo()
            {
                CategoryName = c.LocalisedNames.Localise(locale),
                AttributeInfo = c.CategoryAttributes
                    .OrderBy(ca => ca.AttributeOrder)
                    .Select(ca => new CategoryAttributeInfo()
                    {
                        Id = ca.Attribute.Id,
                        Name = ca.Attribute.AttributeNameLocales.Localise(locale),
                        Searchable = ca.Attribute.Searchable,
                        Sortable = ca.Attribute.Sortable,
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
                        Name = a.AttributeValueList!.LocalisedNames.Localise(locale),
                        Entries = a.AttributeValueList!.ListEntries.Select(e => new AttributeValueListEntryItem()
                        {
                            Id = e.Id,
                            Name = e.LocalisedNames.Localise(locale),
                            OrderIndex = e.OrderIndex
                        }),
                    })
            })
            .FirstAsync();

        return result;
    }
}