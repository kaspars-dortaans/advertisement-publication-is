using BusinessLogic.Constants;
using BusinessLogic.Dto.Advertisement;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;
using BusinessLogic.Entities.Files;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.CookieSettings;
using BusinessLogic.Helpers.FilePathResolver;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Z.EntityFramework.Plus;

namespace BusinessLogic.Services;

public class AdvertisementService(
    Context context,
    ICategoryService categoryService,
    IBaseService<AdvertisementBookmark> advertisementBookmarkService,
    CookieSettingsHelper cookieSettingHelper,
    IFilePathResolver filePathResolver,
    ImageHelper imageHelper) : BaseService<Advertisement>(context), IAdvertisementService
{
    private readonly ICategoryService _categoryService = categoryService;
    private readonly IBaseService<AdvertisementBookmark> _advertisementBookmarkService = advertisementBookmarkService;
    private readonly CookieSettingsHelper _cookieSettingHelper = cookieSettingHelper;
    private readonly IFilePathResolver _filePathResolver = filePathResolver;
    private readonly ImageHelper _imageHelper = imageHelper;
    public async Task<AdvertisementDto> FindOwnedOrActiveAdvertisement(int advertisementId, int? userId)
    {
        var locale = _cookieSettingHelper.Settings.NormalizedLocale;
        var result = await Where(a => a.OwnerId == userId || (a.ValidToDate > DateTime.UtcNow && a.IsActive))
            .Select(a => new AdvertisementDto()
            {
                Id = a.Id,
                AdvertisementText = a.AdvertisementText,
                Title = a.Title,
                CategoryId = a.CategoryId,
                PostedDate = a.PostedDate,
                ViewCount = a.ViewCount,
                MaskedAdvertiserEmail = a.Owner.IsEmailPublic ? a.Owner.Email : null,
                MaskedAdvertiserPhoneNumber = a.Owner.IsPhoneNumberPublic ? a.Owner.PhoneNumber : null,
                OwnerId = a.OwnerId,
                ImageIds = a.Images.Select(i => i.Id),
                Attributes = a.AttributeValues.Select(v => new AttributeValueItem
                {
                    AttributeId = v.AttributeId,
                    AttributeName = v.Attribute.AttributeNameLocales.Localise(locale),
                    Value = v.Value,
                    ValueName = v.Attribute.ValueType == Enums.ValueTypes.ValueListEntry && v.Attribute.AttributeValueList != null
                        ? v.Attribute.AttributeValueList.ListEntries.First(entry => entry.Id == Convert.ToInt16(v.Value)).LocalisedNames.Localise(locale)
                        : null
                }),
                IsBookmarked = userId == null ? null : a.BookmarksOwners.Any(o => o.Id == userId)
            })
            .FirstOrDefaultAsync(a => a.Id == advertisementId)
            ?? throw new ApiException([CustomErrorCodes.NotFound]);

        //Mask email
        if (result.MaskedAdvertiserEmail is not null)
        {
            var email = result.MaskedAdvertiserEmail;
            var atIndex = email.IndexOf('@');
            var revealedCharacterCount = AdvertiserInfoMask.EmailRevealedCharacterCount * 2 > atIndex ? atIndex - AdvertiserInfoMask.EmailRevealedCharacterCount : AdvertiserInfoMask.EmailRevealedCharacterCount;
            result.MaskedAdvertiserEmail = string.Concat(email.AsSpan(0, revealedCharacterCount), new string(AdvertiserInfoMask.MaskChar, atIndex - revealedCharacterCount), email.AsSpan(atIndex));
        }

        //Mask phone number
        if (result.MaskedAdvertiserPhoneNumber is not null)
        {
            var number = result.MaskedAdvertiserPhoneNumber;
            var maskedCharacterCount = AdvertiserInfoMask.PhoneNumberMaskedCharacterCount > number.Length ? number.Length : AdvertiserInfoMask.PhoneNumberMaskedCharacterCount;
            var revealedCharacterCount = number.Length - maskedCharacterCount;
            result.MaskedAdvertiserPhoneNumber = string.Concat(number.AsSpan(0, revealedCharacterCount), new string(AdvertiserInfoMask.MaskChar, maskedCharacterCount));
        }

        return result;
    }

    //TODO: Later check query performance and improve if necessary
    public Task<DataTableQueryResponse<AdvertisementListItemDto>> GetActiveAdvertisementsByCategory(AdvertisementQuery request)
    {
        var advertisementQuery = GetBaseFilteredAdvertisements(request);
        return ResolveAdvertisementDataTableRequest(request, advertisementQuery);
    }

    public Task<DataTableQueryResponse<AdvertisementListItemDto>> GetBookmarkedAdvertisements(AdvertisementQuery request, int currentUserId)
    {
        var advertisementQuery = GetBaseFilteredAdvertisements(request)
            .Where(a => a.BookmarksOwners.Any(o => o.Id == currentUserId));

        return ResolveAdvertisementDataTableRequest(request, advertisementQuery);
    }

    public IQueryable<int> GetCategoryListFromAdvertisementIds(IEnumerable<int> ids)
    {
        return GetActiveAdvertisements()
            .Where(a => ids.Contains(a.Id))
            .GroupBy(a => a.CategoryId)
            .Select(g => g.Key);
    }

    /// <summary>
    /// Select <see cref="AdvertisementListItemDto"/> from <see cref="Advertisement"/>
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public IQueryable<AdvertisementListItemDto> SelectListItemDto(IQueryable<Advertisement> query)
    {
        var locale = _cookieSettingHelper.Settings.NormalizedLocale;
        return query
            .Select(a => new AdvertisementListItemDto()
            {
                Id = a.Id,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.LocalisedNames.Localise(locale),
                PostedDate = a.PostedDate,
                Title = a.Title,
                AdvertisementText = a.AdvertisementText,
                ThumbnailImageId = a.ThumbnailImageId,
                AttributeValues = a.AttributeValues
                    .OrderBy(v => v.Attribute.CategoryAttributes.First(ca => ca.CategoryId == v.Advertisement.CategoryId).AttributeOrder)
                    .Select(v => new AttributeValueItem
                    {
                        AttributeId = v.AttributeId,
                        AttributeName = v.Attribute.AttributeNameLocales.Localise(locale),
                        Value = v.Value,
                        ValueName = v.Attribute.ValueType == Enums.ValueTypes.ValueListEntry && v.Attribute.AttributeValueList != null
                        ? v.Attribute.AttributeValueList.ListEntries.First(entry => entry.Id == Convert.ToInt16(v.Value)).LocalisedNames.Localise(locale)
                        : null
                    })
            });
    }

    /// <summary>
    /// Resolve advertisement data table request with provided advertisement query
    /// </summary>
    /// <param name="request"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    private async Task<DataTableQueryResponse<AdvertisementListItemDto>> ResolveAdvertisementDataTableRequest(AdvertisementQuery request, IQueryable<Advertisement> query)
    {
        var dto = await SelectListItemDto(query).ResolveDataTableQuery(request, new DataTableQueryConfig<AdvertisementListItemDto>()
        {
            AdditionalFilter = query => FilterByAttributes(query, request.AttributeSearch.ToList()),
            AdditionalSort = (query, isSortApplied) => OrderByAttributes(query, isSortApplied, request.AttributeOrder.ToList())
        });

        //In memory sorting
        if (request.AdvertisementIds is not null && !request.AttributeOrder.Any())
        {
            var ids = request.AdvertisementIds.ToList();
            dto.Data = dto.Data.OrderBy(a => ids.IndexOf(a.Id));
        }

        return dto;
    }

    /// <summary>
    /// Returns filtered advertisements which are allowed to be shown publicly
    /// </summary>
    /// <returns></returns>
    private IQueryable<Advertisement> GetActiveAdvertisements()
    {
        return DbSet.Where(a => a.ValidToDate > DateTime.UtcNow && a.IsActive);
    }

    /// <summary>
    /// Get advertisements filtered by AdvertisementQuery basic filters
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    private IQueryable<Advertisement> GetBaseFilteredAdvertisements(AdvertisementQuery request)
    {
        var advertisementQuery = GetActiveAdvertisements()
                    .Filter(request.AdvertisementOwnerId, a => a.OwnerId == request.AdvertisementOwnerId)
                    .Filter(request.AdvertisementIds, a => request.AdvertisementIds!.Contains(a.Id));

        if (request.CategoryId is not null)
        {
            var childCategoryIds = _categoryService.GetCategoryChildIds(request.CategoryId.Value);
            advertisementQuery = advertisementQuery.Where(a => a.CategoryId == request.CategoryId.Value || childCategoryIds.Contains(a.CategoryId));
        }
        return advertisementQuery;
    }

    private IQueryable<AdvertisementListItemDto> FilterByAttributes(IQueryable<AdvertisementListItemDto> query, List<AttributeSearchQuery> attributeSearch)
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
    private static IQueryable<AdvertisementListItemDto> OrderByAttributes(
        IQueryable<AdvertisementListItemDto> query,
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
        Expression<Func<AdvertisementListItemDto, string>> getKeySelectorExpression(int index)
        {
            return a => a.AttributeValues.First(v => v.AttributeId == attributeOrder[index].AttributeId).Value;
        }

        // If no order applied already call orderBy
        var orderedQuery = (IOrderedQueryable<AdvertisementListItemDto>)query;
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

    public async Task BookmarkAdvertisement(int advertisementId, int userId, bool addBookmark)
    {
        if (addBookmark)
        {
            await _advertisementBookmarkService.AddIfNotExistsAsync(new AdvertisementBookmark()
            {
                BookmarkedAdvertisementId = advertisementId,
                BookmarkOwnerId = userId
            });
        }
        else
        {
            await _advertisementBookmarkService
                .DeleteWhereAsync(ab => ab.BookmarkOwnerId == userId && ab.BookmarkedAdvertisementId == advertisementId);
        }
    }

    /// <summary>
    /// Returns all advertisement info which owner id is equal to passed userId
    /// </summary>
    /// <param name="tableQuery"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<DataTableQueryResponse<AdvertisementInfo>> GetAdvertisementInfo(DataTableQuery tableQuery, int? userId)
    {
        var locale = _cookieSettingHelper.Settings.Locale;
        var advertisementInfoQuery = DbSet
            .Filter(userId, a => a.OwnerId == userId)
            .Select(a => new AdvertisementInfo()
            {
                Id = a.Id,
                Title = a.Title,
                CategoryName = a.Category.LocalisedNames.Localise(locale),
                IsActive = a.IsActive,
                ValidTo = a.ValidToDate,
                CreatedAt = a.PostedDate
            });

        return await advertisementInfoQuery.ResolveDataTableQuery(tableQuery);
    }

    public async Task CreateAdvertisement(CreateOrEditAdvertisementDto dto, int userId)
    {
        //Validate selected category
        var canAddToCategory = await DbContext.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.CanContainAdvertisements);
        if (!canAddToCategory)
        {
            throw new ApiException([], new Dictionary<string, IList<string>>
            {
                { nameof(CreateOrEditAdvertisementDto.CategoryId), [CustomErrorCodes.CategoryCanNotContainAdvertisements] }
            });
        }

        //Validate attribute values (in memory)
        var submittedAttributeIds = dto.AttributeValues.Select(av => av.Key).ToList();
        var attributes = await DbContext.Attributes
            .Where(a => a.UsedInCategories.Any(c => c.Id == dto.CategoryId) && submittedAttributeIds.Contains(a.Id))
            .Select(a => new
            {
                a.Id,
                a.ValueType,
                a.ValueValidationRegex,
                ValueListEntryIds = a.AttributeValueList != null ? a.AttributeValueList.ListEntries.Select(e => e.Id.ToString()) : null
            }).ToListAsync();

        List<KeyValuePair<int, string>> invalidAttributes = [];
        var attributeValues = dto.AttributeValues.ToArray();
        for (var i = 0; i < attributeValues.Length; i++)
        {
            var attribute = attributes.FirstOrDefault(a => a.Id == attributeValues[i].Key);
            if (attribute is null)
            {
                continue;
            }

            if (attribute.ValueType == Enums.ValueTypes.ValueListEntry && attribute.ValueListEntryIds is not null)
            {
                //Validate value list selection
                if (attribute.ValueListEntryIds.All(id => id != attributeValues[i].Value))
                {
                    invalidAttributes.Add(new(i, CustomErrorCodes.OptionNotFound));
                }
            }
            else if (!string.IsNullOrEmpty(attribute.ValueValidationRegex))
            {
                //Validate with regex if present
                if (!Regex.IsMatch(attributeValues[i].Value, attribute.ValueValidationRegex))
                {
                    invalidAttributes.Add(new(i, CustomErrorCodes.InvalidValue));
                }
            }
        };

        if (invalidAttributes.Count != 0)
        {
            var validationErrors = invalidAttributes
                .GroupBy(ia => ia.Key)
                .ToDictionary(
                    g => nameof(CreateOrEditAdvertisementDto.AttributeValues) + "[" + g.Key + "]",
                    g => (IList<string>)g.Select(ia => ia.Value).ToList());

            throw new ApiException([], validationErrors);
        }

        var advertisementAttributeValues = dto.AttributeValues
            .Select(av => new AdvertisementAttributeValue()
            {
                AttributeId = av.Key,
                Value = av.Value
            })
            .ToList();

        var newAdvertisement = new Advertisement()
        {
            Title = dto.Title,
            AdvertisementText = dto.Description,
            CategoryId = dto.CategoryId,
            OwnerId = userId,
            AttributeValues = advertisementAttributeValues,
            PostedDate = DateTime.UtcNow,
            ValidToDate = DateTime.UtcNow + new TimeSpan(dto.PostDayCount, 0, 0, 0),
            ViewCount = 0,
            IsActive = true
        };

        var entityEntry = await DbSet.AddAsync(newAdvertisement);
        await DbContext.SaveChangesAsync();

        //Add and store images after successful advertisement creation
        if (dto.ImagesToAdd is not null && dto.ImagesToAdd.Any())
        {
            var imageTasks = dto.ImagesToAdd
                .Select(async (formImage) =>
                {
                    using var imageStream = formImage.OpenReadStream();
                    var imageHash = await FileHelper.GetFileHash(imageStream);
                    var imagePath = _filePathResolver.GenerateUniqueFilePath(FileFolderConstants.AdvertisementImageFolder, formImage.FileName);
                    var thumbnailPath = _filePathResolver.GenerateUniqueFilePath(FileFolderConstants.AdvertisementImageFolder, ImageConstants.ThumbnailPrefix + formImage.FileName);

                    await _imageHelper.StoreImageWithThumbnail(imageStream, imagePath, thumbnailPath);

                    return new AdvertisementImage()
                    {
                        AdvertisementId = entityEntry.Entity.Id,
                        IsPublic = true,
                        Path = imagePath,
                        ThumbnailPath = thumbnailPath,
                        Hash = imageHash
                    };
                })
                .ToArray();

            var images = await Task.WhenAll(imageTasks);
            await DbContext.AdvertisementImages.AddRangeAsync(images);
            await DbContext.SaveChangesAsync();

            //Set thumbnail image
            var thumbnailImage = images.FirstOrDefault(i => i.Hash == dto.ThumbnailImageHash);
            if (thumbnailImage is not null)
            {
                await DbSet.Where(a => a.Id == newAdvertisement.Id).UpdateFromQueryAsync(a => new Advertisement() { ThumbnailImageId = thumbnailImage.Id });
            }
        }
    }
}
