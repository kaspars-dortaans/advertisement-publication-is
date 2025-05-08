using BusinessLogic.Constants;
using BusinessLogic.Dto.Advertisement;
using BusinessLogic.Dto.Category;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.CookieSettings;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public class CategoryService(Context dbContext,
    CookieSettingsHelper cookieSettingsHelper) : BaseService<Category>(dbContext), ICategoryService
{
    private readonly CookieSettingsHelper _cookieSettingsHelper = cookieSettingsHelper;

    public IQueryable<int> GetCategoryChildIds(int categoryId)
    {
        return DbContext.GetCategoryChildIds(categoryId).Select(x => x.Id);
    }

    public IQueryable<int> GetParentCategoryIds(int categoryId)
    {
        return DbContext.GetCategoryParentIds(categoryId).Select(x => x.Id);
    }

    public IQueryable<CategoryAttribute> GetCategoryAndParentAttributes(int categoryId)
    {
        var parentCategoryIds = GetParentCategoryIds(categoryId);
        return DbContext.CategoryAttributes
            .Where(ca => ca.CategoryId == categoryId || parentCategoryIds.Contains(ca.CategoryId));
    }

    public IQueryable<int> GetCategoryListFromAdvertisementIds(IEnumerable<int> ids)
    {
        return AdvertisementService.GetActiveAdvertisements(DbContext.Advertisements)
            .Where(a => ids.Contains(a.Id))
            .GroupBy(a => a.CategoryId)
        .Select(g => g.Key);
    }


    public async Task<CategoryInfo> GetCategoryInfo(int categoryId)
    {
        var locale = _cookieSettingsHelper.Settings.NormalizedLocale;
        var categoryAttributes = GetCategoryAndParentAttributes(categoryId);
        return await
            Where(c => c.Id == categoryId)
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
                        IconName = ca.Attribute.IconName
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
            .FirstOrDefaultAsync() ?? throw new ApiException([CustomErrorCodes.NotFound]);
    }

    public async Task<CategoryAttributeListData> GetCategoryFormInfo(int categoryId)
    {
        var locale = _cookieSettingsHelper.Settings.NormalizedLocale;
        var categoryAttributes = GetCategoryAndParentAttributes(categoryId);
        return new CategoryAttributeListData()
        {
            AttributeInfo = await categoryAttributes
                .OrderBy(ca => ca.AttributeOrder)
                .Select(ca => new AttributeFormInfo()
                {
                    Id = ca.Attribute.Id,
                    Name = ca.Attribute.AttributeNameLocales.Localise(locale),
                    ValueListId = ca.Attribute.AttributeValueListId,
                    AttributeValueType = ca.Attribute.ValueType,
                    IconName = ca.Attribute.IconName,
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
    }

    public async Task UpdateCategory(Category category)
    {
        var existingCategory = (await DbSet
            .Include(c => c.LocalisedNames)
            .Include(c => c.CategoryAttributes)
            .FirstOrDefaultAsync(c => c.Id == category.Id)) 
            ?? throw new ApiException([CustomErrorCodes.NotFound]);
        
        if (existingCategory.LocalisedNames != null && existingCategory.LocalisedNames.Count > 0)
        {
            LocalisationHelper.SyncLocaleTexts(existingCategory.LocalisedNames, category.LocalisedNames);
        } else
        {
            existingCategory.LocalisedNames = category.LocalisedNames;
        }

        existingCategory.LocalisedNames = category.LocalisedNames;
        existingCategory.ParentCategoryId = category.ParentCategoryId;
        existingCategory.CanContainAdvertisements = category.CanContainAdvertisements;
        existingCategory.CategoryAttributes = category.CategoryAttributes;

        await UpdateAsync(existingCategory);
    }
}