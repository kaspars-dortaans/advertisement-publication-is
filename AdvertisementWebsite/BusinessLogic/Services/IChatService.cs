using BusinessLogic.Dto.Message;
using BusinessLogic.Entities;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services;

public interface IChatService : IBaseService<Chat>
{
    public Task<IEnumerable<ChatListItem>> GetUserChats(int userId);
    public Task<ChatListItem> CreateChat(int userId, int toUserId, Message firstMessage, int? advertisementId, IEnumerable<IFormFile>? Attachments = null);
    public Task<MessageItem> SendMessage(int chatId, int userId, string text, IEnumerable<IFormFile>? Attachments);
    public Task<int> MarkMessageAsRead(int chatId, IEnumerable<int> messageIds, int userId);
    public Task<IEnumerable<int>> ChatRecipientIds(int chatId);
    public Task<IEnumerable<int>> MessageRecipientIds(int messageId);
}
