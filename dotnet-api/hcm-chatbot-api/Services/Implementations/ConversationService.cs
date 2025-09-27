using Microsoft.EntityFrameworkCore;
using Data;
using Repositories;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implementations;

public class ConversationService : IConversationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<conversation> _conversationRepository;

    public ConversationService(IUnitOfWork unitOfWork, IGenericRepository<conversation> conversationRepository)
    {
        _unitOfWork = unitOfWork;
        _conversationRepository = conversationRepository;
    }

    public async Task<IEnumerable<conversation>> GetUserConversationsAsync(Guid userId)
    {
        var conversations = await _conversationRepository.GetAllAsync();
        return conversations.Where(c => c.user_id == userId).OrderByDescending(c => c.updated_at);
    }

    public async Task<conversation?> GetConversationByIdAsync(Guid id)
    {
        return await _conversationRepository.GetByIdAsync(id);
    }

    public async Task<conversation> CreateConversationAsync(conversation conversation)
    {
        conversation.id = Guid.NewGuid();
        conversation.created_at = DateTime.UtcNow;
        conversation.updated_at = DateTime.UtcNow;
        conversation.message_count ??= 0;

        var result = await _conversationRepository.AddAsync(conversation);
        await _unitOfWork.CompleteAsync();
        return result;
    }

    public async Task<conversation> UpdateConversationAsync(conversation conversation)
    {
        conversation.updated_at = DateTime.UtcNow;
        await _conversationRepository.UpdateAsync(conversation);
        await _unitOfWork.CompleteAsync();
        return conversation;
    }

    public async Task<bool> DeleteConversationAsync(Guid id)
    {
        var result = await _conversationRepository.DeleteAsync(id);
        if (result)
        {
            await _unitOfWork.CompleteAsync();
        }
        return result;
    }

    public async Task<IEnumerable<conversation>> GetRecentConversationsAsync(int count = 10)
    {
        var conversations = await _conversationRepository.GetAllAsync();
        return conversations.OrderByDescending(c => c.created_at).Take(count);
    }

    public async Task UpdateConversationMessageCountAsync(Guid conversationId)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null) return;

        conversation.message_count = (conversation.message_count ?? 0) + 1;
        conversation.updated_at = DateTime.UtcNow;

        await _conversationRepository.UpdateAsync(conversation);
        await _unitOfWork.CompleteAsync();
    }
}