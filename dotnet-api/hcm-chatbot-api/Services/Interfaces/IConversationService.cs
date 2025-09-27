using Data;

namespace Services.Interfaces;

public interface IConversationService
{
    Task<IEnumerable<conversation>> GetUserConversationsAsync(Guid userId);
    Task<conversation?> GetConversationByIdAsync(Guid id);
    Task<conversation> CreateConversationAsync(conversation conversation);
    Task<conversation> UpdateConversationAsync(conversation conversation);
    Task<bool> DeleteConversationAsync(Guid id);
    Task<IEnumerable<conversation>> GetRecentConversationsAsync(int count = 10);
    Task UpdateConversationMessageCountAsync(Guid conversationId);
}