using Data;

namespace Services.Interfaces;

public interface IMessageService
{
    Task<IEnumerable<message>> GetConversationMessagesAsync(Guid conversationId);
    Task<message?> GetMessageByIdAsync(Guid id);
    Task<message> CreateMessageAsync(message message);
    Task<message> UpdateMessageAsync(message message);
    Task<bool> DeleteMessageAsync(Guid id);
    Task<IEnumerable<message>> GetRecentMessagesAsync(int count = 50);
    Task<int> GetTotalMessagesCountAsync();
}