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
using Web.Helpers;

namespace Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class AdvertisementController(
    IMapper mapper,
    IBaseService<Category> categoryService,
    IAdvertisementService advertisementService,
    IBaseService<RuleViolationReport> ruleViolationService,
    CookieSettingsHelper cookieSettingsHelper) : ControllerBase
{
    private readonly IMapper _mapper = mapper;
    private readonly IBaseService<Category> _categoryService = categoryService;
    private readonly IAdvertisementService _advertisementService = advertisementService;
    private readonly IBaseService<RuleViolationReport> _ruleViolationService = ruleViolationService;
    private readonly CookieSettingsHelper _cookieSettingsHelper = cookieSettingsHelper;

    [AllowAnonymous]
    [HttpPost]
    public async Task<DataTableQueryResponse<AdvertisementListItem>> GetAdvertisements(AdvertisementQuery request)
    {
        var result = await _advertisementService.GetActiveAdvertisementsByCategory(request);
        return result.MapDataTableResponse<AdvertisementListItemDto, AdvertisementListItem>(_mapper, opts => opts.Items[nameof(Url)] = Url);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IEnumerable<AdvertisementListItem>> GetAdvertisementsByIds(List<int> ids)
    {
        var advertisements = _advertisementService
            .GetActiveAdvertisements()
            .Where(a => ids.Contains(a.Id));

        var dto = (await _advertisementService.SelectListItemDto(advertisements).ToListAsync())
                .OrderBy(a => ids.IndexOf(a.Id));

        return _mapper.Map<IEnumerable<AdvertisementListItem>>(dto, o => o.Items[nameof(Url)] = Url);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<Dto.Advertisement.AdvertisementDto> GetAdvertisement(int advertisementId)
    {
        var dto = await _advertisementService.FindActiveAdvertisement(advertisementId, User.GetUserId());
        return _mapper.Map<Dto.Advertisement.AdvertisementDto>(dto, opts => opts.Items[nameof(Url)] = Url);
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbidResult))]
    public async Task<IActionResult> RevealAdvertiserPhoneNumber(int advertisementId)
    {
        var queryResult = await _advertisementService
            .Where(a => a.Id == advertisementId)
            .Select(a => new { IsPublic = a.Owner.IsPhoneNumberPublic, a.Owner.PhoneNumber })
            .FirstAsync();

        if (!queryResult.IsPublic)
        {
            return Forbid();
        }

        return new OkObjectResult(queryResult.PhoneNumber);
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbidResult))]
    public async Task<IActionResult> RevealAdvertiserEmail(int advertisementId)
    {
        var queryResult = await _advertisementService
            .Where(a => a.Id == advertisementId)
            .Select(a => new { IsPublic = a.Owner.IsEmailPublic, a.Owner.Email })
            .FirstAsync();

        if (!queryResult.IsPublic)
        {
            return Forbid();
        }

        return new OkObjectResult(queryResult.Email);
    }

    [HttpPost]
    public async Task<IActionResult> BookmarkAdvertisement(BookmarkAdvertisementRequest request)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }
        await _advertisementService.BookmarkAdvertisement(request.AdvertisementId, userId.Value, request.AddBookmark);
        return Ok();
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

    [AllowAnonymous]
    [HttpPost]
    public async Task ReportAdvertisement(ReportAdvertisementRequest request)
    {
        var report = _mapper.Map<RuleViolationReport>(request, o => o.Items[nameof(User)] = User);
        await _ruleViolationService.AddAsync(report);
    }
}