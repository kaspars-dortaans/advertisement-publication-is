using BusinessLogic.Dto.Advertisement;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.CookieSettings;
using System.Linq.Expressions;

namespace BusinessLogic.Services;

public class AdvertisementService(
    Context context,
    ICategoryService categoryService,
    CookieSettingsHelper cookieSettingHelper) : BaseService<Advertisement>(context), IAdvertisementService
{
    private readonly ICategoryService _categoryService = categoryService;
    private readonly CookieSettingsHelper _cookieSettingHelper = cookieSettingHelper;

    //TODO: Later check query performance and improve if necessary
    public async Task<DataTableQueryResponse<AdvertisementListItem>> GetActiveAdvertisementsByCategory(AdvertisementQuery request)
    {
        var advertisementQuery = DbSet.Where(a => a.ValidToDate > DateTime.UtcNow);
        if (request.CategoryId is not null)
        {
            var childCategoryIds = _categoryService.GetCategoryChildIds(request.CategoryId.Value);
            advertisementQuery = advertisementQuery.Where(a => a.CategoryId == request.CategoryId.Value || childCategoryIds.Contains(a.CategoryId));
        }

        var locale = _cookieSettingHelper.Settings.NormalizedLocale;
        var advertisementItemQuery = advertisementQuery
            .Select(a => new AdvertisementListItem()
            {
                Id = a.Id,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.LocalisedNames.Localise(locale),
                PostedDate = a.PostedDate,
                Title = a.Title,
                AdvertisementText = a.AdvertisementText,
                ThumbnailImagePath = a.ThumbnailImage == null ? null : a.ThumbnailImage.Path,
                AttributeValues = a.AttributeValues.OrderBy(v => v.Attribute.CategoryAttributes.First(ca => ca.CategoryId == v.Advertisement.CategoryId).AttributeOrder).Select(v => new AttributeValueItem
                {
                    AttributeId = v.AttributeId,
                    AttributeName = v.Attribute.AttributeNameLocales.Localise(locale),
                    Value = v.Value,
                    ValueName = v.Attribute.ValueType == Enums.ValueTypes.ValueListEntry && v.Attribute.AttributeValueList != null
                        ? v.Attribute.AttributeValueList.ListEntries.First(entry => entry.Id == Convert.ToInt16(v.Value)).LocalisedNames.Localise(locale)
                        : null
                })
            });

        return await advertisementItemQuery.ResolveDataTableQuery(request, new DataTableQueryConfig<AdvertisementListItem>()
        {
            AdditionalFilter = query => FilterByAttributes(query, request.AttributeSearch.ToList()),
            AdditionalSort = (query, isSortApplied) => OrderByAttributes(query, isSortApplied, request.AttributeOrder.ToList())
        });
    }

    private IQueryable<AdvertisementListItem> FilterByAttributes(IQueryable<AdvertisementListItem> query, List<AttributeSearchQuery> attributeSearch)
    {
        foreach (var search in attributeSearch)
        {
            if (string.IsNullOrEmpty(search.Value))
            {
                continue;
            }

            var isValueInt = int.TryParse(search.Value, out var valueInt);
            var isSecondaryValueInt = int.TryParse(search.SecondaryValue, out var secondaryValueInt);

            query = query
                .Where(advertisement =>
                    DbContext.Attributes.First(attribute => attribute.Id == search.AttributeId).FilterType == Enums.FilterType.FromTo
                    ? DbContext.Attributes.First(attribute => attribute.Id == search.AttributeId).ValueType == Enums.ValueTypes.ValueListEntry
                        //From to search
                        //For value lists  
                        ? ((!isValueInt || DbContext.AttributeValueListEntries.First(entry => entry.Id == Convert.ToInt32(advertisement.AttributeValues.First(attributeValue => attributeValue.AttributeId == search.AttributeId).Value)).OrderIndex >= valueInt)
                            && (!isSecondaryValueInt || DbContext.AttributeValueListEntries.First(entry => entry.Id == Convert.ToInt32(advertisement.AttributeValues.First(attributeValue => attributeValue.AttributeId == search.AttributeId).Value)).OrderIndex <= secondaryValueInt))
                        //For integers
                        : ((!isValueInt || Convert.ToInt32(advertisement.AttributeValues.First(attributeValue => attributeValue.AttributeId == search.AttributeId).Value) >= valueInt)
                            && (!isSecondaryValueInt || Convert.ToInt32(advertisement.AttributeValues.First(attributeValue => attributeValue.AttributeId == search.AttributeId).Value) <= secondaryValueInt))

                    : DbContext.Attributes.First(attribute => attribute.Id == search.AttributeId).FilterType == Enums.FilterType.Search
                    //Contains search
                    ? advertisement.AttributeValues.First(attributeValue => attributeValue.AttributeId == search.AttributeId).Value.Contains(search.Value)
                    //Match
                    : advertisement.AttributeValues.First(attributeValue => attributeValue.AttributeId == search.AttributeId).Value == search.Value);
        }
        return query;
    }

    //TODO: Test ordering for different attribute types
    private static IQueryable<AdvertisementListItem> OrderByAttributes(
        IQueryable<AdvertisementListItem> query,
        bool sortApplied,
        List<AttributeOrderQuery> attributeOrder)
    {
        if (attributeOrder.Count < 1)
        {
            return query;
        }

        var orderIndex = 0;
        // New expression is returned in order to capture current index into expression scope
        // Otherwise expression references variable, which upon expression evaluation will be changed and identical for all expression calls
        Expression<Func<AdvertisementListItem, string>> getKeySelectorExpression(int index)
        {
            return a => a.AttributeValues.First(v => v.AttributeId == attributeOrder[index].AttributeId).Value;
        }

        // If no order applied already call orderBy
        var orderedQuery = (IOrderedQueryable<AdvertisementListItem>)query;
        if (!sortApplied)
        {
            orderedQuery = attributeOrder[orderIndex].Direction == Direction.Ascending
                ? query.OrderBy(getKeySelectorExpression(orderIndex))
                : query.OrderByDescending(getKeySelectorExpression(orderIndex));
            orderIndex++;
        }

        // If already applied, add secondary sort
        for (; orderIndex < attributeOrder.Count; orderIndex++)
        {
            orderedQuery = attributeOrder[orderIndex].Direction == Direction.Ascending
                ? orderedQuery.ThenBy(getKeySelectorExpression(orderIndex))
                : orderedQuery.ThenByDescending(getKeySelectorExpression(orderIndex));
        }

        return orderedQuery;
    }
}
