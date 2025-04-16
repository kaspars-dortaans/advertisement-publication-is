using Web.Dto.Message;

namespace Web.Hubs;

public interface IMessageHub
{
    public Task SendNewMessage(int chatId, MessageItemDto message);
    public Task SendNewChat(ChatListItemDto chat);
    public Task MarkMessageAsRead(int chatId, IEnumerable<int> messageId);
}
