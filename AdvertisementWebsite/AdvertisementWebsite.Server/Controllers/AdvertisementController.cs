using AdvertisementWebsite.Server.Dto.Advertisement;
using AdvertisementWebsite.Server.Dto.Common;
using AdvertisementWebsite.Server.Helpers;
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

namespace AdvertisementWebsite.Server.Controllers;

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
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
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
        await _advertisementBookmarkService.DeleteWhereAsync(ab => ab.BookmarkOwnerId == userId && ids.Contains(ab.BookmarkedAdvertisementId));
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
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
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
    public async Task SetIsActiveOwnedAdvertisements(SetActiveStatusRequest request)
    {
        var userId = User.GetUserId() ?? throw new ApiException([CustomErrorCodes.UserNotFound]);
        await _advertisementService
            .Where(a => a.OwnerId == userId && request.Ids.Contains(a.Id))
            .UpdateFromQueryAsync(a => new Advertisement() { IsActive = request.IsActive });
    }

    [HasPermission(Permissions.DeleteOwnedAdvertisement)]
    [HttpPost]
    public async Task DeleteOwnedAdvertisements(IEnumerable<int> advertisementIds)
    {
        await _advertisementService.RemoveAdvertisements(advertisementIds, User.GetUserId()!.Value);
    }

    [HasPermission(Permissions.CreateOwnedAdvertisement)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<int> CreateAdvertisement([FromForm] CreateOrEditAdvertisementRequest request)
    {
        var userId = User.GetUserId()!.Value;
        var advertisementDto = _mapper.Map<CreateOrEditAdvertisementDto>(request);
        return await _advertisementService.CreateAdvertisement(advertisementDto, userId);
    }

    [HasPermission(Permissions.EditOwnedAdvertisement)]
    [ProducesResponseType<AdvertisementFormInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<AdvertisementFormInfo> EditAdvertisement(int advertisementId)
    {
        var dto = await _advertisementService.GetAdvertisementFormInfo(advertisementId, User.GetUserId()!.Value);
        var advertisementValues = _mapper.Map<CreateOrEditAdvertisementRequest>(dto, opts => opts.Items[nameof(Url)] = Url);
        return new AdvertisementFormInfo
        {
            Advertisement = advertisementValues,
            CategoryInfo = await _categoryService.GetCategoryFormInfo(dto.CategoryId)
        };
    }

    [HasPermission(Permissions.EditOwnedAdvertisement)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task EditAdvertisement([FromForm] CreateOrEditAdvertisementRequest request)
    {
        var dto = _mapper.Map<CreateOrEditAdvertisementDto>(request);
        await _advertisementService.UpdateAdvertisement(dto, User.GetUserId()!.Value);
    }

    [ProducesResponseType<IEnumerable<KeyValuePair<int, string>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IEnumerable<KeyValuePair<int, string>>> GetAdvertisementLookupByIds(IEnumerable<int> ids)
    {
        return await _advertisementService
            .Where(a => ids.Contains(a.Id))
            .Select(a => new KeyValuePair<int, string>(a.Id, a.Title))
            .ToListAsync();
    }

    [HasPermission(Permissions.ViewAllAdvertisements)]
    [ProducesResponseType<DataTableQueryResponse<AdvertisementInfo>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<DataTableQueryResponse<AdvertisementInfo>> GetAllAdvertisements(DataTableQuery request)
    {
        return await _advertisementService.GetAdvertisementInfo(request);
    }

    [HasPermission(Permissions.ViewAllAdvertisements)]
    [ProducesResponseType<Dto.Advertisement.AdvertisementDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<Dto.Advertisement.AdvertisementDto> GetAnyAdvertisement(int advertisementId)
    {
        var dto = await _advertisementService.FindAdvertisement(advertisementId, User.GetUserId());
        return _mapper.Map<Dto.Advertisement.AdvertisementDto>(dto, opts => opts.Items[nameof(Url)] = Url);
    }

    [HasPermission(Permissions.EditAnyAdvertisement)]
    [HttpPost]
    public async Task SetIsActiveAnyAdvertisements(SetActiveStatusRequest request)
    {
        await _advertisementService
            .Where(a => request.Ids.Contains(a.Id))
            .UpdateFromQueryAsync(a => new Advertisement() { IsActive = request.IsActive });
    }

    [HasPermission(Permissions.CreateAdvertisement)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<int> CreateAdvertisementForAnyUser([FromForm] CreateOrEditAdvertisementRequest request)
    {
        var advertisementDto = _mapper.Map<CreateOrEditAdvertisementDto>(request);
        return await _advertisementService.CreateAdvertisement(advertisementDto, request.OwnerId ?? User.GetUserId()!.Value, true);
    }

    [HasPermission(Permissions.EditAnyAdvertisement)]
    [ProducesResponseType<AdvertisementFormInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<AdvertisementFormInfo> EditAnyAdvertisement(int advertisementId)
    {
        var dto = await _advertisementService.GetAdvertisementFormInfo(advertisementId);
        var advertisementValues = _mapper.Map<CreateOrEditAdvertisementRequest>(dto, opts => opts.Items[nameof(Url)] = Url);
        return new AdvertisementFormInfo
        {
            Advertisement = advertisementValues,
            CategoryInfo = await _categoryService.GetCategoryFormInfo(dto.CategoryId)
        };
    }

    [HasPermission(Permissions.EditAnyAdvertisement)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task EditAnyAdvertisement([FromForm] CreateOrEditAdvertisementRequest request)
    {
        var dto = _mapper.Map<CreateOrEditAdvertisementDto>(request);
        await _advertisementService.UpdateAdvertisement(dto);
    }

    [HasPermission(Permissions.EditAnyAdvertisement)]
    [ProducesResponseType<AdvertisementFormInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task ExtendAdvertisements(ExtendRequest request)
    {
        await _advertisementService.ExtendAdvertisement(request.Ids, request.ExtendTime);
    }


    [HasPermission(Permissions.DeleteAnyAdvertisement)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task DeleteAnyAdvertisements(IEnumerable<int> ids)
    {
        await _advertisementService.RemoveAdvertisements(ids);
    }
}