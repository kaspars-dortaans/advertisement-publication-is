using AdvertisementWebsite.Server.Dto.Category;
using BusinessLogic.Dto.Category;
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
    CookieSettingsHelper cookieSettingsHelper
    ) : ControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;
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
    public async Task<CategoryFormInfo> GetCategoryFormInfo(int categoryId)
    {
        return await _categoryService.GetCategoryFormInfo(categoryId);
    }

}
