using AutoMapper;
using BusinessLogic.Authorization;
using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Dto.AdvertisementNotifications;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdvertisementWebsite.Server.Dto.AdvertisementNotification;
using AdvertisementWebsite.Server.Dto.Common;
using AdvertisementWebsite.Server.Helpers;

namespace AdvertisementWebsite.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class AdvertisementNotificationController(
    IAdvertisementNotificationSubscriptionService subscriptionService,
    ICategoryService categoryService,
    IMapper mapper
    ) : ControllerBase
{
    private readonly IAdvertisementNotificationSubscriptionService _subscriptionService = subscriptionService;
    private readonly ICategoryService _categoryService = categoryService;
    private readonly IMapper _mapper = mapper;

    [HasPermission(Permissions.ViewOwnedAdvertisementNotificationSubscriptions)]
    [ProducesResponseType<DataTableQueryResponse<NotificationSubscriptionItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<DataTableQueryResponse<NotificationSubscriptionItem>> GetAdvertisementNotificationSubscriptions(DataTableQuery query)
    {
        var userId = User.GetUserId()!.Value;
        return await _subscriptionService.GetSubscriptions(query, userId);
    }

    [HasPermission(Permissions.ViewOwnedAdvertisementNotificationSubscriptions)]
    [ProducesResponseType<IEnumerable<KeyValuePair<int, string>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IEnumerable<KeyValuePair<int, string>>> GetSubscriptionsLookupByIds(IEnumerable<int> ids)
    {
        var userId = User.GetUserId()!.Value;
        return await _subscriptionService.GetLookupByIds(ids, userId).ToListAsync();
    }

    [HasPermission(Permissions.CreateOwnedAdvertisementNotificationSubscription)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<int> CreateSubscriptions(CreateOrEditNotificationSubscriptionRequest request)
    {
        var userId = User.GetUserId()!.Value;
        var dto = _mapper.Map<CreateOrEditSubscription>(request);
        return await _subscriptionService.CreateSubscription(dto, userId);
    }

    [HasPermission(Permissions.EditOwnedAdvertisementNotificationSubscriptions)]
    [ProducesResponseType<SubscriptionFormInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<SubscriptionFormInfo> EditSubscriptions(int subscriptionId)
    {
        var userId = User.GetUserId()!.Value;
        var subscriptionFormInfo = await _subscriptionService.GetSubscriptionInfo(subscriptionId, userId);
        return new SubscriptionFormInfo
        {
            Subscription = _mapper.Map<CreateOrEditNotificationSubscriptionRequest>(subscriptionFormInfo),
            CategoryInfo = await _categoryService.GetCategoryFormInfo(subscriptionFormInfo.CategoryId)
        };
    }

    [HasPermission(Permissions.EditOwnedAdvertisementNotificationSubscriptions)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task EditSubscriptions(CreateOrEditNotificationSubscriptionRequest request)
    {
        if (request.Id == null)
        {
            throw new ApiException([], new Dictionary<string, IList<string>>
            {
                { nameof(CreateOrEditNotificationSubscriptionRequest.Id), [CustomErrorCodes.MissingRequired] }
            });
        }

        var userId = User.GetUserId()!.Value;
        var dto = _mapper.Map<CreateOrEditSubscription>(request);
        await _subscriptionService.EditSubscription(dto, userId);
    }

    [HasPermission(Permissions.EditOwnedAdvertisementNotificationSubscriptions)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task SetSubscriptionActiveStatus(SetActiveStatusRequest request)
    {
        var userId = User.GetUserId()!;
        await _subscriptionService
            .Where(s => s.OwnerId == userId && request.Ids.Contains(s.Id))
            .UpdateFromQueryAsync(a => new AdvertisementNotificationSubscription() { IsActive = request.IsActive });
    }

    [HasPermission(Permissions.DeleteOwnedAdvertisementNotificationSubscriptions)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task DeleteSubscriptions(IEnumerable<int> subscriptionIds)
    {
        var userId = User.GetUserId()!.Value;
        await _subscriptionService.DeleteWhereAsync(s => s.OwnerId == userId && subscriptionIds.Contains(s.Id));
    }

    [HasPermission(Permissions.EditAnyAdvertisementNotificationSubscription)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task ExtendSubscription(ExtendRequest request)
    {
        await _subscriptionService.ExtendSubscriptions(request.Ids, request.ExtendTime);
    }
}
