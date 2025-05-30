using AutoMapper;
using BusinessLogic.Authorization;
using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Dto.Message;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers.CookieSettings;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using AdvertisementWebsite.Server.Dto.Message;
using AdvertisementWebsite.Server.Helpers;
using AdvertisementWebsite.Server.Hubs;

namespace AdvertisementWebsite.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class MessageController(
        IChatService chatService,
        IBaseService<Message> messageService,
        IHubContext<MessageHub, IMessageHub> messageHubContext,
        IMapper mapper,
        CookieSettingsHelper cookieSettingHelper
    ) : ControllerBase
{
    private readonly IChatService _chatService = chatService;
    private readonly IBaseService<Message> _messageService = messageService;
    private readonly IHubContext<MessageHub, IMessageHub> _messageHubContext = messageHubContext;
    private readonly IMapper _mapper = mapper;
    private readonly CookieSettingsHelper _cookieSettingsHelper = cookieSettingHelper;

    [HasPermission(Permissions.ViewMessages)]
    [ProducesResponseType<IEnumerable<ChatListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<IEnumerable<ChatListItemDto>> GetAllChats()
    {
        var userId = User.GetUserId()!.Value;
        var items = await _chatService.GetUserChats(userId);
        return _mapper.Map<IEnumerable<ChatListItemDto>>(items, opts => opts.Items[nameof(Url)] = Url);
    }

    [HasPermission(Permissions.ViewMessages)]
    [ProducesResponseType<IDictionary<DateTime, IEnumerable<MessageItemDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<IDictionary<DateTime, IEnumerable<MessageItemDto>>> GetChatMessages(int chatId)
    {
        var userId = User!.GetUserId()!.Value;
        var messages = await _chatService.Where(c =>
                c.Id == chatId
                && c.Users.Any(m => m.Id == userId)
            )
            .Select(c => c.ChatMessages
                .Select(m => new MessageItem
                {
                    Id = m.Id,
                    FromUserId = m.FromUserId,
                    Text = m.Text,
                    SentTime = m.SentTime,
                    IsMessageRead = m.IsMessageRead,
                    Attachments = m.Attachments.Select(a => new MessageAttachmentItem
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        SizeInBytes = a.SizeInBytes,
                    })
                })
            )
            .FirstOrDefaultAsync()
            ?? throw new ApiException([CustomErrorCodes.NotFound]);

        var messageDTOs = _mapper.Map<IEnumerable<MessageItemDto>>(messages, opts => opts.Items[nameof(Url)] = Url);

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(_cookieSettingsHelper.Settings.TimeZoneId, out TimeZoneInfo? userTimeZoneInfo))
        {
            userTimeZoneInfo = TimeZoneInfo.Utc;
        }
        return messageDTOs
            .GroupBy(m => TimeZoneInfo.ConvertTimeFromUtc(m.SentTime, userTimeZoneInfo).Date)
            .ToDictionary(g => g.Key, g => g.OrderBy(m => m.SentTime).AsEnumerable());
    }

    [HasPermission(Permissions.SendMessage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task CreateChat([FromForm] CreateChatRequest rq)
    {
        var userId = User!.GetUserId()!.Value;
        var newChat = await _chatService.CreateChat(userId, rq.UserId, new Message
        {
            FromUserId = userId,
            IsMessageRead = false,
            Text = rq.WithMessage.Text,
            SentTime = DateTime.UtcNow,
        }, rq.AdvertisementId, rq.WithMessage.Attachments);

        var recipientIds = (await _chatService.ChatRecipientIds(newChat.Id)).Select(id => id.ToString());
        if (recipientIds.Any())
        {
            var newChatDto = _mapper.Map<ChatListItemDto>(newChat, opts => opts.Items[nameof(Url)] = Url);

            //Send "new chat" event for messages receiver
            await _messageHubContext.Clients.Users(recipientIds).SendNewChat(newChatDto);
        }
    }

    [HasPermission(Permissions.SendMessage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task SendMessage([FromForm] SendMessageRequest rq)
    {
        var userId = User!.GetUserId()!.Value;

        //Save message in db
        var messageItem = await _chatService.SendMessage(rq.ChatId, userId, rq.Text, rq.Attachments);

        var recipientIds = (await _chatService.ChatRecipientIds(rq.ChatId)).Select(id => id.ToString());
        if (recipientIds.Any())
        {
            var messageDto = _mapper.Map<MessageItemDto>(messageItem, opts => opts.Items[nameof(Url)] = Url);
            //Send "new message" event for message recipients
            await _messageHubContext.Clients.Users(recipientIds).SendNewMessage(rq.ChatId, messageDto);
        }
    }

    [HasPermission(Permissions.ViewMessages)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task MarkMessageAsRead(int chatId, IEnumerable<int> messageIds)
    {
        //To implement group chats, this endpoint need to be adapted to support group chat message read status change
        var userId = User!.GetUserId()!.Value;
        var messagesAffected = await _chatService.MarkMessageAsRead(chatId, messageIds, userId);

        var recipientIds = (await _chatService.MessageRecipientIds(messageIds.First())).Select(id => id.ToString());
        if (recipientIds.Any())
        {
            //Send "mark message as read" event for message recipients
            await _messageHubContext.Clients.Users(recipientIds).MarkMessageAsRead(chatId, userId, messageIds, messagesAffected);
        }
    }

    [HasPermission(Permissions.ViewMessages)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<int> GetUnreadMessageCount()
    {
        var userId = User.GetUserId()!.Value;
        return await _messageService.CountAsync(m => !m.IsMessageRead && m.FromUserId != userId && m.Chat.ChatUsers.Any(u => u.UserId == userId));
    }
}
