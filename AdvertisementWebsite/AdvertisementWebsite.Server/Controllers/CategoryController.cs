using AdvertisementWebsite.Server.Dto.Category;
using AutoMapper;
using BusinessLogic.Authorization;
using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Dto.Category;
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
public class CategoryController(
    ICategoryService categoryService,
    IBaseService<BusinessLogic.Entities.Attribute> attributeService,
    IMapper mapper,
    CookieSettingsHelper cookieSettingsHelper
    ) : ControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;
    private readonly IBaseService<BusinessLogic.Entities.Attribute> _attributeService = attributeService;
    private readonly IMapper _mapper = mapper;
    private readonly CookieSettingsHelper _cookieSettingsHelper = cookieSettingsHelper;

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
    public async Task<IEnumerable<KeyValuePair<int, string>>> GetCategoryLookup()
    {
        return await _categoryService
            .GetAll()
            .Select(c => new KeyValuePair<int, string>(c.Id, c.LocalisedNames.Localise(_cookieSettingsHelper.Settings.NormalizedLocale)))
            .ToListAsync();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IEnumerable<KeyValuePair<int, string>>> GetAttributeLookup()
    {
        return await _attributeService
            .GetAll()
            .Select(a => new KeyValuePair<int, string>(a.Id, a.AttributeNameLocales.Localise(_cookieSettingsHelper.Settings.NormalizedLocale)))
            .ToListAsync();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<CategoryInfo> GetCategoryInfo(int categoryId)
    {
        return await _categoryService.GetCategoryInfo(categoryId);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IEnumerable<KeyValuePair<int, string>>> GetCategoryListFromAdvertisementIds(IEnumerable<int> advertisementIds)
    {
        var advertisementCategoryIds = _categoryService.GetCategoryListFromAdvertisementIds(advertisementIds);
        var locale = _cookieSettingsHelper.Settings.NormalizedLocale;
        var categoryList = await _categoryService
            .Where(c => advertisementCategoryIds.Contains(c.Id))
            .Select(c => new KeyValuePair<int, string>(c.Id, c.LocalisedNames.Localise(locale)))
            .ToListAsync();

        return categoryList;
    }


    [AllowAnonymous]
    [HttpGet]
    public async Task<CategoryAttributeListData> GetCategoryAttributeInfo(int categoryId)
    {
        return await _categoryService.GetCategoryFormInfo(categoryId);
    }

    [HasPermission(Permissions.CreateCategory)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task CreateCategory(PutCategoryRequest request)
    {
        var newCategory = _mapper.Map<Category>(request);
        await _categoryService.AddAsync(newCategory);
    }

    [HasPermission(Permissions.ViewCategories)]
    [ProducesResponseType<PutCategoryRequest>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<PutCategoryRequest> GetCategoryFormInfo(int categoryId)
    {
        return (await _categoryService
            .Where(c => c.Id == categoryId)
            .Select(c => new PutCategoryRequest
            {
                Id = c.Id,
                CanContainAdvertisements = c.CanContainAdvertisements,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.LocalisedNames.Localise(_cookieSettingsHelper.Settings.Locale) : null,
                LocalizedNames = (IEnumerable<KeyValuePair<string, string>?>)c.LocalisedNames.Select(ln => new KeyValuePair<string, string>(ln.Locale, ln.Text)),
                CategoryAttributeOrder = c.CategoryAttributes
                    .OrderBy(ca => ca.AttributeOrder)
                    .Select(ca => new KeyValuePair<int, string>(
                        ca.AttributeId,
                        ca.Attribute.AttributeNameLocales.Localise(_cookieSettingsHelper.Settings.NormalizedLocale)))

            })
            .FirstOrDefaultAsync())
            ?? throw new ApiException([CustomErrorCodes.NotFound]);
    }

    [HasPermission(Permissions.EditCategory)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task EditCategory(PutCategoryRequest request)
    {
        if (request.Id == null)
        {
            throw new ApiException([], new Dictionary<string, IList<string>>
            {
                { nameof(request.Id), [CustomErrorCodes.MissingRequired] }
            });
        }

        var category = _mapper.Map<Category>(request);
        await _categoryService.UpdateCategory(category);
    }

    [HasPermission(Permissions.DeleteCategory)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task DeleteCategory(IEnumerable<int> ids)
    {
        await _categoryService.DeleteWhereAsync(c => ids.Contains(c.Id));
    }
}
