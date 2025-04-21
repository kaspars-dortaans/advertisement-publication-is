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
using Web.Dto.AdvertisementNotification;
using Web.Dto.Common;
using Web.Helpers;

namespace Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class AdvertisementNotificationController(
    IAdvertisementNotificationSubscriptionService subscriptionService,
    IAdvertisementService advertisementService,
    IMapper mapper
    ) : ControllerBase
{
    private readonly IAdvertisementNotificationSubscriptionService _subscriptionService = subscriptionService;
    private readonly IAdvertisementService _advertisementService = advertisementService;
    private readonly IMapper _mapper = mapper;

    [HasPermission(Permissions.ViewAdvertisementNotificationSubscriptions)]
    [ProducesResponseType<DataTableQueryResponse<NotificationSubscriptionItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<DataTableQueryResponse<NotificationSubscriptionItem>> GetAdvertisementNotificationSubscriptions(DataTableQuery query)
    {
        var userId = User.GetUserId()!.Value;
        return await _subscriptionService.GetSubscriptions(query, userId);
    }

    [HasPermission(Permissions.ViewAdvertisementNotificationSubscriptions)]
    [ProducesResponseType<IEnumerable<KeyValuePair<int, string>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IEnumerable<KeyValuePair<int, string>>> GetSubscriptionsLookupByIds(IEnumerable<int> ids)
    {
        var userId = User.GetUserId()!.Value;
        return await _subscriptionService.GetLookupByIds(ids, userId).ToListAsync();
    }

    [HasPermission(Permissions.CreateAdvertisementNotificationSubscription)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task CreateSubscriptions(CreateOrEditNotificationSubscriptionRequest request)
    {
        if (request.PaidTime == null)
        {
            throw new ApiException([], new Dictionary<string, IList<string>>
            {
                { nameof(CreateOrEditNotificationSubscriptionRequest.PaidTime), [CustomErrorCodes.MissingRequired] }
            });
        }

        var userId = User.GetUserId()!.Value;
        var dto = _mapper.Map<CreateOrEditSubscription>(request);
        await _subscriptionService.CreateSubscription(dto, userId);
    }

    [HasPermission(Permissions.EditAdvertisementNotificationSubscriptions)]
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
            CategoryInfo = await _advertisementService.GetCategoryFormInfo(subscriptionFormInfo.CategoryId)
        };
    }

    [HasPermission(Permissions.EditAdvertisementNotificationSubscriptions)]
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

    [HasPermission(Permissions.EditAdvertisementNotificationSubscriptions)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task ExtendSubscriptions(ExtendRequest request)
    {
        var userId = User.GetUserId()!.Value;
        await _subscriptionService.ExtendSubscriptions(request.Ids, request.ExtendTime, userId);
    }
         
    [HasPermission(Permissions.EditAdvertisementNotificationSubscriptions)]
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

    [HasPermission(Permissions.DeleteAdvertisementNotificationSubscriptions)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task DeleteSubscriptions(IEnumerable<int> subscriptionIds)
    {
        var userId = User.GetUserId()!.Value;
        await _subscriptionService.DeleteWhereAsync(s => s.OwnerId == userId && subscriptionIds.Contains(s.Id));
    }
}
