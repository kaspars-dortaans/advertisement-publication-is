using BusinessLogic.Constants;
using BusinessLogic.Dto.Advertisement;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Image;
using BusinessLogic.Dto.Time;
using BusinessLogic.Entities;
using BusinessLogic.Entities.Files;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.BackgroundJobs;
using BusinessLogic.Helpers.CookieSettings;
using BusinessLogic.Helpers.FilePathResolver;
using BusinessLogic.Helpers.Storage;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Z.EntityFramework.Plus;

namespace BusinessLogic.Services;

public class AdvertisementService(
    Context context,
    ICategoryService categoryService,
    IBaseService<AdvertisementBookmark> advertisementBookmarkService,
    IBaseService<Chat> chatService,
    IAttributeValidatorService attributeValidatorService,
    CookieSettingsHelper cookieSettingHelper,
    IFilePathResolver filePathResolver,
    IStorage storage,
    ImageHelper imageHelper) : BaseService<Advertisement>(context), IAdvertisementService
{
    private readonly ICategoryService _categoryService = categoryService;
    private readonly IBaseService<AdvertisementBookmark> _advertisementBookmarkService = advertisementBookmarkService;
    private readonly IBaseService<Chat> _chatService = chatService;
    private readonly IAttributeValidatorService _attributeValidatorService = attributeValidatorService;
    private readonly CookieSettingsHelper _cookieSettingHelper = cookieSettingHelper;
    private readonly IFilePathResolver _filePathResolver = filePathResolver;
    private readonly IStorage _storage = storage;
    private readonly ImageHelper _imageHelper = imageHelper;

    public Task<AdvertisementDto> FindOwnedOrActiveAdvertisement(int advertisementId, int? userId)
    {
        var query = Where(a => a.OwnerId == userId || (a.ValidToDate > DateTime.UtcNow && a.IsActive));
        return FindAdvertisement(query, advertisementId, userId);
    }

    public Task<AdvertisementDto> FindAdvertisement(int advertisementId, int? userId)
    {
        return FindAdvertisement(DbSet, advertisementId, userId);
    }

    private async Task<AdvertisementDto> FindAdvertisement(IQueryable<Advertisement> query, int advertisementId, int? userId)
    {
        var locale = _cookieSettingHelper.Settings.NormalizedLocale;
        var result = (await query
            .Select(a => new AdvertisementDto()
            {
                Id = a.Id,
                AdvertisementText = a.AdvertisementText,
                Title = a.Title,
                CategoryId = a.CategoryId,
                CreatedDate = a.CreatedDate,
                ViewCount = a.ViewCount,
                MaskedAdvertiserEmail = a.Owner.IsEmailPublic ? a.Owner.Email : null,
                MaskedAdvertiserPhoneNumber = a.Owner.IsPhoneNumberPublic ? a.Owner.PhoneNumber : null,
                OwnerId = a.OwnerId,
                ImageIds = a.Images.OrderBy(i => i.Order).Select(i => i.Id),
                Attributes = a.AttributeValues.Select(v => new AttributeValueItem
                {
                    AttributeId = v.AttributeId,
                    AttributeName = v.Attribute.AttributeNameLocales.Localise(locale),
                    Value = v.Value,
                    IconName = v.Attribute.IconName,
                    ValueName = v.Attribute.ValueType == Enums.ValueTypes.ValueListEntry && v.Attribute.AttributeValueList != null
                        ? v.Attribute.AttributeValueList.ListEntries.FirstOrDefault(entry => entry.Id == Convert.ToInt16(v.Value)) != null
                            ? v.Attribute.AttributeValueList.ListEntries.FirstOrDefault(entry => entry.Id == Convert.ToInt16(v.Value))!.LocalisedNames.Localise(locale)
                            : LocalisationConstants.NotLocalizedTextPlaceholder
                        : null
                }),
                IsBookmarked = userId == null ? null : a.BookmarksOwners.Any(o => o.Id == userId)
            })
            .FirstOrDefaultAsync(a => a.Id == advertisementId))
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
                //Ef could not translate OrderBy with Localise extension method
                CategoryName = a.Category.LocalisedNames.FirstOrDefault(lt => lt.Locale == locale) != null
                    ? a.Category.LocalisedNames.FirstOrDefault(lt => lt.Locale == locale)!.Text
                    : LocalisationConstants.NotLocalizedTextPlaceholder,
                CreatedDate = a.CreatedDate,
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
                        IconName = v.Attribute.IconName,
                        ValueName = v.Attribute.ValueType == Enums.ValueTypes.ValueListEntry && v.Attribute.AttributeValueList != null
                            ? v.Attribute.AttributeValueList.ListEntries.First(entry => entry.Id == Convert.ToInt16(v.Value)).LocalisedNames.Localise(locale)
                            : null,
                        ShowOnListItem = v.Attribute.ShowOnListItem
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

        //In memory filter
        foreach(var a in dto.Data)
        {
            a.AttributeValues = a.AttributeValues.Where(av => av.ShowOnListItem).ToList();
        }
        
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
    public static IQueryable<Advertisement> GetActiveAdvertisements(IQueryable<Advertisement> query)
    {
        return query.Where(a => a.ValidToDate.HasValue && a.ValidToDate.Value > DateTime.UtcNow && a.IsActive);
    }

    /// <summary>
    /// Get advertisements filtered by AdvertisementQuery basic filters
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    private IQueryable<Advertisement> GetBaseFilteredAdvertisements(AdvertisementQuery request)
    {
        var advertisementQuery = GetActiveAdvertisements(DbSet)
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

                    : (DbContext.Attributes.First(attribute => attribute.Id == search.AttributeId).FilterType == Enums.FilterType.Search
                        //Contains search
                        ? advertisement.AttributeValues.First(attributeValue => attributeValue.AttributeId == search.AttributeId).Value.Contains(search.Value)
                        //Match
                        : advertisement.AttributeValues.First(attributeValue => attributeValue.AttributeId == search.AttributeId).Value == search.Value));
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
    public async Task<DataTableQueryResponse<AdvertisementInfo>> GetAdvertisementInfo(DataTableQuery tableQuery, int? userId = null)
    {
        var locale = _cookieSettingHelper.Settings.Locale;
        var advertisementInfoQuery = DbSet
            .Filter(userId, a => a.OwnerId == userId)
            .Select(a => new AdvertisementInfo()
            {
                Id = a.Id,
                OwnerUsername = userId == null ? a.Owner.UserName : null,
                Title = a.Title,
                //Ef could not translate OrderBy with Localise extension method
                CategoryName = a.Category.LocalisedNames.FirstOrDefault(lt => lt.Locale == locale) != null
                    ? a.Category.LocalisedNames.FirstOrDefault(lt => lt.Locale == locale)!.Text
                    : LocalisationConstants.NotLocalizedTextPlaceholder,
                Status = a.ValidToDate == null
                    ? Enums.PaymentSubjectStatus.Draft
                    : (a.ValidToDate < DateTime.UtcNow
                        ? Enums.PaymentSubjectStatus.Expired
                        : a.IsActive
                            ? Enums.PaymentSubjectStatus.Active
                            : Enums.PaymentSubjectStatus.Inactive),
                ValidToDate = a.ValidToDate,
                CreatedAtDate = a.CreatedDate
            });

        return await advertisementInfoQuery.ResolveDataTableQuery(tableQuery);
    }

    public async Task RemoveAdvertisements(IEnumerable<int> advertisementIds, int? userId = null)
    {
        var advertisementQuery = Where(a => advertisementIds.Contains(a.Id))
            .Filter(userId, a => a.OwnerId == userId);

        var imagePaths = (await advertisementQuery
            .SelectMany(a => a.Images.Select(i => new[] { i.Path, i.ThumbnailPath }))
            .ToListAsync())
            .SelectMany(p => p);

        var attachmentPaths = await _chatService
            .Where(c => advertisementQuery.Any(a => a.Id == c.AdvertisementId))
            .SelectMany(c => c.ChatMessages.SelectMany(m => m.Attachments.Select(a => a.Path))).ToListAsync();

        await _chatService.DeleteWhereAsync(c => advertisementQuery.Any(a => a.Id == c.AdvertisementId));
        await Task.WhenAll([
            _storage.DeleteFiles(imagePaths.Concat(attachmentPaths)),
            advertisementQuery.ExecuteDeleteAsync()
        ]);
    }

    public async Task<int> CreateAdvertisement(CreateOrEditAdvertisementDto dto, int userId, bool setValidToDate = false)
    {
        //Validate
        await _attributeValidatorService.ValidateAttributeCategory(dto.CategoryId, nameof(CreateOrEditAdvertisementDto.CategoryId));
        await _attributeValidatorService.ValidateAdvertisementAttributeValues(dto.AttributeValues, dto.CategoryId, nameof(CreateOrEditAdvertisementDto.AttributeValues));
        var advertisementAttributeValues = dto.AttributeValues.Select(av => new AdvertisementAttributeValue
        {
            AttributeId = av.Key,
            Value = av.Value,
        }).ToList();

        //Add new entities
        var advertisement = new Advertisement()
        {
            Title = dto.Title,
            AdvertisementText = dto.Description,
            CategoryId = dto.CategoryId,
            OwnerId = userId,
            AttributeValues = advertisementAttributeValues,
            ViewCount = 0,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        if (setValidToDate)
        {
            advertisement.ValidToDate = DateTime.UtcNow.AddDays(dto.PostTime.ToDays());
        }

        await AddAsync(advertisement);
        await SynchronizeAdvertisementImages(advertisement.Id, dto.ImagesToAdd, dto.ImageOrder, null);
        await UpdateAdvertisementThumbnailImage(advertisement.Id, dto.ImageOrder?.FirstOrDefault()?.Hash);

        if (setValidToDate)
        {
            BackgroundJob.Enqueue<IAdvertisementNotificationSender>((s) => s.SendNotifications(advertisement.Id));
        }

        return advertisement.Id;
    }

    public async Task UpdateAdvertisement(CreateOrEditAdvertisementDto dto, int? ownerId = null)
    {
        //Make sure user is advertisement owner, and it exists while necessary data to compare against
        var compareData = Where(a => a.Id == dto.Id)
            .Filter(ownerId, a => a.OwnerId == ownerId)
            .Select(a => new
            {
                a.CategoryId,
                AttributeAndValueIds = a.AttributeValues.Select(av => new KeyValuePair<int, int>(av.AttributeId, av.Id)),
                a.Images
            })
            .FirstOrDefault() ?? throw new ApiException([CustomErrorCodes.NotFound]);

        //Validate
        if (dto.CategoryId != compareData.CategoryId)
        {
            await _attributeValidatorService.ValidateAttributeCategory(dto.CategoryId, nameof(CreateOrEditAdvertisementDto.CategoryId));
        }
        await _attributeValidatorService.ValidateAdvertisementAttributeValues(dto.AttributeValues, dto.CategoryId, nameof(CreateOrEditAdvertisementDto.AttributeValues));
        var attributeValues = dto.AttributeValues.Select(av => new AdvertisementAttributeValue
        {
            AttributeId = av.Key,
            Value = av.Value
        }).ToList();

        //Updated entities
        await SynchronizeAdvertisementAttributeValues(dto.Id!.Value, attributeValues, compareData.AttributeAndValueIds.ToList());
        await SynchronizeAdvertisementImages(dto.Id!.Value, dto.ImagesToAdd, dto.ImageOrder, compareData.Images);

        var thumbnailHash = dto.ImageOrder?.FirstOrDefault()?.Hash;
        await Where(a => a.Id == dto.Id!.Value).UpdateFromQueryAsync(a => new Advertisement()
        {
            CategoryId = dto.CategoryId,
            Title = dto.Title,
            OwnerId = ownerId ?? dto.OwnerId!.Value,
            AdvertisementText = dto.Description,
            ThumbnailImageId = string.IsNullOrEmpty(thumbnailHash) ? null : DbContext.AdvertisementImages.FirstOrDefault(ai => ai.AdvertisementId == dto.Id && ai.Hash == thumbnailHash)!.Id
        });
    }

    private async Task SynchronizeAdvertisementAttributeValues(int advertisementId, List<AdvertisementAttributeValue> attributeValues, List<KeyValuePair<int, int>> existingAttributeValues)
    {
        var newAttributes = new List<AdvertisementAttributeValue>();
        var updatedAttributes = new List<AdvertisementAttributeValue>();
        var removedAttributeIds = existingAttributeValues.Select(existingAv => existingAv.Value).ToHashSet();

        foreach (var av in attributeValues)
        {
            av.AdvertisementId = advertisementId;
            var existingAv = existingAttributeValues.FirstOrDefault(existingAv => existingAv.Key == av.AttributeId, new KeyValuePair<int, int>(-1, -1));
            if (existingAv.Key < 0)
            {
                newAttributes.Add(av);
            }
            else
            {
                av.Id = existingAv.Value;
                updatedAttributes.Add(av);
                removedAttributeIds.Remove(av.Id);
            }
        }

        //Add new attributes
        await DbContext.AddRangeAsync(newAttributes);
        await DbContext.SaveChangesAsync();

        //Update attributes
        DbContext.AdvertisementAttributeValues.UpdateRange(updatedAttributes);
        await DbContext.SaveChangesAsync();

        //Remove attributes
        await DbContext.AdvertisementAttributeValues.Where(av => removedAttributeIds.Contains(av.Id)).ExecuteDeleteAsync();
    }

    private async Task SynchronizeAdvertisementImages(int advertisementId, IEnumerable<IFormFile>? imageFiles, IEnumerable<ImageDto>? imageOrder, IEnumerable<AdvertisementImage>? existingImages)
    {
        var newImages = new List<AdvertisementImage>();
        var existingImageList = existingImages?.ToList() ?? [];
        var imageOrderList = imageOrder?.ToList() ?? [];
        if (imageFiles is not null && imageFiles.Any())
        {
            var newImageTasks = imageFiles
            .Select(async (formImage) =>
            {
                using var imageStream = formImage.OpenReadStream();
                var imageHash = await FileHelper.GetFileHash(imageStream);

                if (existingImageList.Any(i => i.Hash == imageHash))
                {
                    return null;
                }

                var order = imageOrderList.FindIndex(i => i.Hash == imageHash);
                var imagePath = _filePathResolver.GenerateUniqueFilePath(FileFolderConstants.AdvertisementImageFolder, formImage.FileName);
                var thumbnailPath = _filePathResolver.GenerateUniqueFilePath(FileFolderConstants.AdvertisementImageFolder, ImageConstants.ThumbnailPrefix + formImage.FileName);

                await _imageHelper.StoreImageWithThumbnail(imageStream, imagePath, thumbnailPath);

                return new AdvertisementImage()
                {
                    AdvertisementId = advertisementId,
                    IsPublic = true,
                    Path = imagePath,
                    ThumbnailPath = thumbnailPath,
                    Hash = imageHash,
                    Order = order < 0 ? int.MaxValue : order
                };
            });
            newImages = (await Task.WhenAll(newImageTasks)).Where(i => i is not null)!.ToList<AdvertisementImage>();

            //Add
            if (newImages.Count > 0)
            {
                await DbContext.AdvertisementImages.AddRangeAsync(newImages);
                await DbContext.SaveChangesAsync();
            }
        }

        //Split existing images into updated and deleted
        var updatedImages = new List<AdvertisementImage>();
        var deletedImages = new List<AdvertisementImage>();
        for (int i = 0; i < existingImageList.Count; i++)
        {
            var order = imageOrderList.FindIndex(o => o.Hash == existingImageList[i].Hash);
            if (order < 0)
            {
                deletedImages.Add(existingImageList[i]);
            }
            else
            {
                existingImageList[i].Order = order;
                updatedImages.Add(existingImageList[i]);
            }
        }

        //Update
        if (updatedImages.Count > 0)
        {
            DbContext.AdvertisementImages.UpdateRange(updatedImages);
            await DbContext.SaveChangesAsync();
        }

        //Delete
        if (deletedImages.Count > 0)
        {
            var deletedImagePaths = deletedImages
                .SelectMany(di => new[] { di.Path, di.ThumbnailPath })
                .Where(path => !string.IsNullOrEmpty(path))
                .ToList();
            var deletedImageIds = deletedImages.Select(di => di.Id).ToList();

            await Task.WhenAll([
                DbContext.AdvertisementImages
                    .Where(ai => deletedImageIds.Contains(ai.Id))
                    .ExecuteDeleteAsync(),
                _storage.DeleteFiles(deletedImagePaths)
            ]);
        }
    }

    private async Task UpdateAdvertisementThumbnailImage(int advertisementId, string? thumbnailHash)
    {
        if (!string.IsNullOrEmpty(thumbnailHash))
        {
            await DbSet
                .Where(a => a.Id == advertisementId)
                .UpdateFromQueryAsync(a => new Advertisement()
                {
                    ThumbnailImageId = DbContext.AdvertisementImages
                        .First(ai => ai.AdvertisementId == advertisementId && ai.Hash == thumbnailHash).Id
                });
        }
    }

    public async Task<CreateOrEditAdvertisementDto> GetAdvertisementFormInfo(int advertisementId, int? ownerId = null)
    {
        var dto = (await DbSet
            .Filter(ownerId, a => a.OwnerId == ownerId)
            .Select(a => new CreateOrEditAdvertisementDto
            {
                Id = a.Id,
                OwnerId = a.OwnerId,
                OwnerUsername = a.Owner.UserName,
                CategoryId = a.CategoryId,
                AttributeValues = a.AttributeValues.Select(av => new KeyValuePair<int, string>(av.AttributeId, av.Value)),
                ValidToDate = a.ValidToDate,
                Title = a.Title,
                Description = a.AdvertisementText,
                ImageOrder = a.Images.OrderBy(i => i.Order).Select(i => new ImageDto
                {
                    Id = i.Id,
                    Hash = i.Hash
                })
            })
            .FirstOrDefaultAsync(a => a.Id == advertisementId)) ?? throw new ApiException([CustomErrorCodes.NotFound]);
        return dto;
    }

    public async Task ExtendAdvertisement(IEnumerable<int> advertisementId, PostTimeDto extendTime)
    {
        var advertisements = await Where(a => advertisementId.Contains(a.Id))
            .ToListAsync();

        //Validate that user is owner of all specified advertisements
        if (advertisements.Count != advertisementId.Count())
        {
            throw new ApiException([CustomErrorCodes.NotFound]);
        }

        foreach (var advertisement in advertisements)
        {
            if (advertisement.ValidToDate == null || DateTime.UtcNow > advertisement.ValidToDate)
            {
                advertisement.ValidToDate = DateTime.UtcNow.AddDays(extendTime.ToDays());
            }
            else
            {
                advertisement.ValidToDate = advertisement.ValidToDate.Value.AddDays(extendTime.ToDays());
            }
        }
        await DbContext.SaveChangesAsync();
    }
}
