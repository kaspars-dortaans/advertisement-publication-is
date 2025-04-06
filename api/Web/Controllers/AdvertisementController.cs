using AutoMapper;
using BusinessLogic.Authorization;
using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Dto.Advertisement;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.CookieSettings;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Dto.Advertisement;
using Web.Dto.User;
using Web.Helpers;

namespace Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class AdvertisementController(
    IMapper mapper,
    ICategoryService categoryService,
    IAdvertisementService advertisementService,
    IBaseService<RuleViolationReport> ruleViolationService,
    IBaseService<AdvertisementBookmark> advertisementBookmarkService,
    CookieSettingsHelper cookieSettingsHelper) : ControllerBase
{
    private readonly IMapper _mapper = mapper;
    private readonly ICategoryService _categoryService = categoryService;
    private readonly IAdvertisementService _advertisementService = advertisementService;
    private readonly IBaseService<RuleViolationReport> _ruleViolationService = ruleViolationService;
    private readonly IBaseService<AdvertisementBookmark> _advertisementBookmarkService = advertisementBookmarkService;
    private readonly CookieSettingsHelper _cookieSettingsHelper = cookieSettingsHelper;

    [AllowAnonymous]
    [HttpPost]
    public async Task<DataTableQueryResponse<AdvertisementListItem>> GetAdvertisements(AdvertisementQuery request)
    {
        var result = await _advertisementService.GetActiveAdvertisementsByCategory(request);
        return result.MapDataTableResponse<AdvertisementListItemDto, AdvertisementListItem>(_mapper, opts => opts.Items[nameof(Url)] = Url);
    }

    [HasPermission(Permissions.ViewAdvertisementBookmarks)]
    [HttpPost]
    public async Task<DataTableQueryResponse<AdvertisementListItem>> GetBookmarkedAdvertisements(AdvertisementQuery request)
    {
        var result = await _advertisementService.GetBookmarkedAdvertisements(request, User.GetUserId()!.Value);
        return result.MapDataTableResponse<AdvertisementListItemDto, AdvertisementListItem>(_mapper, opts => opts.Items[nameof(Url)] = Url);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<Dto.Advertisement.AdvertisementDto> GetAdvertisement(int advertisementId)
    {
        var dto = await _advertisementService.FindOwnedOrActiveAdvertisement(advertisementId, User.GetUserId());
        return _mapper.Map<Dto.Advertisement.AdvertisementDto>(dto, opts => opts.Items[nameof(Url)] = Url);
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<ForbidResult>(StatusCodes.Status403Forbidden)]
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
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<ForbidResult>(StatusCodes.Status403Forbidden)]
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

    [HasPermission(Permissions.EditAdvertisementBookmark)]
    [ProducesResponseType<PublicUserInfoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task BookmarkAdvertisement(BookmarkAdvertisementRequest request)
    {
        var userId = User.GetUserId()!;
        await _advertisementService.BookmarkAdvertisement(request.AdvertisementId, userId.Value, request.AddBookmark);
    }

    [HasPermission(Permissions.ViewAdvertisementBookmarks)]
    [HttpPost]
    public async Task RemoveAdvertisementBookmarks(IEnumerable<int> ids)
    {
        var userId = User.GetUserId();
        //TODO: Remove advertisement images from storage
        await _advertisementBookmarkService.DeleteWhereAsync(ab => ab.BookmarkOwnerId == userId && ids.Contains(ab.BookmarkedAdvertisementId));
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
        var categoryAttributes = _categoryService.GetCategoryAndParentAttributes(categoryId);
        var result = await _categoryService
            .Where(c => c.Id == categoryId)
            .Select(c => new CategoryInfo()
            {
                CategoryName = c.LocalisedNames.Localise(locale),
                AttributeInfo = categoryAttributes
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
                    }).ToList(),
                AttributeValueLists = categoryAttributes
                    .Select(ca => ca.Attribute)
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
                    .ToList()
            })
            .FirstAsync();

        return result;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IEnumerable<KeyValuePair<int, string>>> GetCategoryListFromAdvertisementIds(IEnumerable<int> advertisementIds)
    {
        var advertisementCategoryIds = _advertisementService.GetCategoryListFromAdvertisementIds(advertisementIds);
        var locale = _cookieSettingsHelper.Settings.NormalizedLocale;
        var categoryList = await _categoryService
            .Where(c => advertisementCategoryIds.Contains(c.Id))
            .Select(c => new KeyValuePair<int, string>(c.Id, c.LocalisedNames.Localise(locale)))
            .ToListAsync();

        return categoryList;
    }

    [HasPermission(Permissions.ViewAdvertisementBookmarks)]
    [HttpGet]
    public async Task<IEnumerable<KeyValuePair<int, string>>> GetBookmarkedAdvertisementCategoryList()
    {
        var userId = User.GetUserId();
        var locale = _cookieSettingsHelper.Settings.Locale;
        var categoryList = await _advertisementService
            .Where(a => a.BookmarksOwners.Any(bo => bo.Id == userId))
            .Select(a => a.Category)
            .Distinct()
            .Select(c => new KeyValuePair<int, string>(c.Id, c.LocalisedNames.Localise(locale)))
            .ToListAsync();

        return categoryList;
    }

    [AllowAnonymous]
    [ProducesResponseType<PublicUserInfoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task ReportAdvertisement(ReportAdvertisementRequest request)
    {
        var report = _mapper.Map<RuleViolationReport>(request, o => o.Items[nameof(User)] = User);
        await _ruleViolationService.AddAsync(report);
    }

    [HasPermission(Permissions.ViewOwnedAdvertisements)]
    [HttpPost]
    public async Task<DataTableQueryResponse<AdvertisementInfo>> GetOwnedAdvertisements(DataTableQuery query)
    {
        var userId = User.GetUserId() ?? throw new ApiException([CustomErrorCodes.UserNotFound]);
        return await _advertisementService.GetAdvertisementInfo(query, userId);
    }

    [HasPermission(Permissions.EditOwnedAdvertisement)]
    [HttpPost]
    public async Task SetIsActiveAdvertisements(IEnumerable<int> advertisementIds, bool isActive)
    {
        var userId = User.GetUserId() ?? throw new ApiException([CustomErrorCodes.UserNotFound]);
        await _advertisementService
            .Where(a => a.OwnerId == userId && advertisementIds.Contains(a.Id))
            .UpdateFromQueryAsync(a => new Advertisement() { IsActive = isActive });
    }

    [HasPermission(Permissions.DeleteOwnedAdvertisement)]
    [HttpPost]
    public async Task DeleteAdvertisements(IEnumerable<int> advertisementIds)
    {
        await _advertisementService.RemoveAdvertisements(advertisementIds, User.GetUserId()!.Value);
    }

    [HasPermission(Permissions.CreateAdvertisement)]
    [ProducesResponseType<PublicUserInfoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task CreateAdvertisement([FromForm] CreateOrEditAdvertisementRequest request)
    {
        var userId = User.GetUserId()!.Value;
        var advertisementDto = _mapper.Map<CreateOrEditAdvertisementDto>(request);
        await _advertisementService.CreateAdvertisement(advertisementDto, userId);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<CategoryFormInfo> GetCategoryFormInfo(int categoryId)
    {
        var locale = _cookieSettingsHelper.Settings.NormalizedLocale;
        var categoryAttributes = _categoryService.GetCategoryAndParentAttributes(categoryId);

        var result = new CategoryFormInfo()
        {
            AttributeInfo = await categoryAttributes
                .OrderBy(ca => ca.AttributeOrder)
                .Select(ca => new AttributeFormInfo()
                {
                    Id = ca.Attribute.Id,
                    Name = ca.Attribute.AttributeNameLocales.Localise(locale),
                    ValueListId = ca.Attribute.AttributeValueListId,
                    AttributeValueType = ca.Attribute.ValueType,
                    IconUrl = ca.Attribute.Icon != null ? ca.Attribute.Icon.Path : null,
                    ValueValidationRegex = ca.Attribute.ValueValidationRegex
                })
                .ToListAsync(),
            AttributeValueLists = await categoryAttributes
                .Select(ca => ca.Attribute)
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
                .ToListAsync()
        };

        return result;
    }
}