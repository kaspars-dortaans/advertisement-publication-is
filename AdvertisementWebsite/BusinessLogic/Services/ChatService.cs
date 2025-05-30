using BusinessLogic.Constants;
using BusinessLogic.Dto.Message;
using BusinessLogic.Entities;
using BusinessLogic.Entities.Files;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.FilePathResolver;
using BusinessLogic.Helpers.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace BusinessLogic.Services;

public class ChatService(Context dbContext,
    IBaseService<Advertisement> advertisementService,
    IBaseService<Message> messageService,
    IStorage storage,
    IFilePathResolver filePathResolver) : BaseService<Chat>(dbContext), IChatService
{
    private readonly IBaseService<Advertisement> _advertisementService = advertisementService;
    private readonly IBaseService<Message> _messageService = messageService;
    private readonly IStorage _storage = storage;
    private readonly IFilePathResolver _filePathResolver1 = filePathResolver;

    public async Task<IEnumerable<ChatListItem>> GetUserChats(int userId)
    {
        var query = Where(c => c.Users.Any(m => m.Id == userId))
            .OrderBy(c => c.ChatMessages.Max(m => m.SentTime));

        return await SelectChatListItems(query, userId).ToListAsync();
    }

    private IQueryable<ChatListItem> SelectChatListItems(IQueryable<Chat> query, int userId)
    {
        var userAdvertisementIds = _advertisementService.Where(a => a.OwnerId == userId).Select(a => a.Id);
        return query.Select(c => new ChatListItem
        {
            Id = c.Id,
            AdvertisementId = c.AdvertisementId,
            AdvertisementOwnerId = c.Advertisement != null ? c.Advertisement.OwnerId : null,
            Title = c.Advertisement != null ? c.Advertisement.Title : c.ChatMessages.First(m => m.FromUserId != userId).FromUser.UserName!,
            UnreadMessageCount = c.ChatMessages.Where(m => m.FromUserId != userId && !m.IsMessageRead).Count(),
            ThumbnailImageId = c.Advertisement != null ? c.Advertisement.ThumbnailImageId : c.ChatMessages.First(m => m.FromUserId != userId).FromUser.ProfileImageFileId,
            LastMessage = c.ChatMessages.OrderByDescending(c => c.SentTime).First().Text
        });
    }

    public async Task<ChatListItem> CreateChat(int userId, int toUserId, Message firstMessage, int? advertisementId, IEnumerable<IFormFile>? attachments = null)
    {
        //Validate advertisement
        if (advertisementId is not null)
        {
            var data = (await _advertisementService
                .Where(a => a.Id == advertisementId)
                .Select(a => new { IsActive = a.ValidToDate > DateTime.UtcNow && a.IsActive, IsOwner = a.OwnerId == toUserId })
                .FirstOrDefaultAsync())
                ?? throw new ApiException([CustomErrorCodes.AdvertisementNotFound]);

            if (!data.IsOwner)
            {
                throw new ApiException([CustomErrorCodes.UserNotOwnerOfAdvertisement]);
            }

            if (!data.IsActive)
            {
                throw new ApiException([CustomErrorCodes.AdvertisementNotActive]);
            }
        }

        //Validate if chat does not already exist
        if (await ExistsAsync(c => c.AdvertisementId == advertisementId 
            && c.ChatUsers.Any(cu => cu.UserId == userId) 
            && c.ChatUsers.Any(c => c.UserId == toUserId)))
        {
            throw new ApiException([CustomErrorCodes.ChatAlreadyExists]);
        }

        //Upload attachments
        firstMessage.Attachments = await UploadAttachmentsToStorage(attachments);

        //Add chat with first message
        var newChatEntry = await AddAsync(new Chat
        {
            AdvertisementId = advertisementId,
            ChatUsers = [
                new()
                {
                    UserId = userId,
                },
                new()
                {
                    UserId = toUserId
                }],
            ChatMessages = [firstMessage],
        });

        //Return chat info for notification recipients
        return await SelectChatListItems(Where(c => c.Id == newChatEntry.Id), toUserId).FirstAsync();
    }

    public async Task<MessageItem> SendMessage(int chatId, int userId, string text, IEnumerable<IFormFile>? attachments)
    {
        //Validate and upload attachments to storage
        await ValidateIsMemberOfChat(chatId, userId);
        var messageAttachments = await UploadAttachmentsToStorage(attachments);

        //Add message to db
        var entry = await _messageService.AddAsync(new Message
        {
            ChatId = chatId,
            FromUserId = userId,
            Text = text,
            SentTime = DateTime.UtcNow,
            IsMessageRead = false,
            Attachments = messageAttachments
        });

        //Return message info for notification recipients
        return new MessageItem
        {
            Id = entry.Id,
            FromUserId = entry.FromUserId,
            Text = entry.Text,
            IsMessageRead = entry.IsMessageRead,
            SentTime = entry.SentTime,
            Attachments = entry.Attachments.Select(a => new MessageAttachmentItem
            {
                Id = a.Id,
                FileName = a.FileName,
                SizeInBytes = a.SizeInBytes
            })
        };
    }

    private async Task<ICollection<MessageAttachment>> UploadAttachmentsToStorage(IEnumerable<IFormFile>? attachments)
    {
        var messageAttachments = Array.Empty<MessageAttachment>();
        if (attachments is not null && attachments.Any())
        {
            var storeTasks = attachments.Select(async (a) =>
            {
                using var fileStream = a.OpenReadStream();
                var newAttachment = new MessageAttachment
                {
                    Path = _filePathResolver1.GenerateUniqueFilePath(FileFolderConstants.MessageAttachmentFolder, a.FileName),
                    FileName = a.FileName,
                    Hash = await FileHelper.GetFileHash(fileStream),
                    IsPublic = false,
                    SizeInBytes = a.Length
                };
                fileStream.Seek(0, SeekOrigin.Begin);
                await _storage.PutFile(newAttachment.Path, fileStream);
                return newAttachment;
            });
            messageAttachments = await Task.WhenAll(storeTasks);
        }
        return messageAttachments;
    }

    public async Task<int> MarkMessageAsRead(int chatId, IEnumerable<int> messageIds, int userId)
    {
        await ValidateIsMemberOfChat(chatId, userId);

        return await _messageService
            .Where(m => messageIds.Contains(m.Id) && userId != m.FromUserId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.IsMessageRead, true));
    }

    private async Task ValidateIsMemberOfChat(int chatId, int userId)
    {
        if (!await ExistsAsync(c => c.Id == chatId && c.Users.Any(m => m.Id == userId)))
        {
            throw new ApiException([CustomErrorCodes.NotAMemberOfChat]);
        }
    }

    public async Task<IEnumerable<int>> ChatRecipientIds(int chatId)
    {
        return await Where(c => c.Id == chatId).Select(c => c.Users.Select(c => c.Id)).FirstOrDefaultAsync() ?? [];
    }

    public async Task<IEnumerable<int>> MessageRecipientIds(int messageId)
    {
        return await _messageService.Where(m => m.Id == messageId).Select(m => m.Chat.Users.Select(member => member.Id)).FirstOrDefaultAsync() ?? [];
    }
}
