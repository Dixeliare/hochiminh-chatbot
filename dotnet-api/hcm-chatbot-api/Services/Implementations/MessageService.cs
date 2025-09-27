using Microsoft.EntityFrameworkCore;
using Data;
using Repositories;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implementations;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<message> _messageRepository;

    public MessageService(IUnitOfWork unitOfWork, IGenericRepository<message> messageRepository)
    {
        _unitOfWork = unitOfWork;
        _messageRepository = messageRepository;
    }

    public async Task<IEnumerable<message>> GetConversationMessagesAsync(Guid conversationId)
    {
        var messages = await _messageRepository.GetAllAsync();
        return messages.Where(m => m.conversation_id == conversationId).OrderBy(m => m.created_at);
    }

    public async Task<message?> GetMessageByIdAsync(Guid id)
    {
        return await _messageRepository.GetByIdAsync(id);
    }

    public async Task<message> CreateMessageAsync(message message)
    {
        message.id = Guid.NewGuid();
        message.created_at = DateTime.UtcNow;

        var result = await _messageRepository.AddAsync(message);
        await _unitOfWork.CompleteAsync();
        return result;
    }

    public async Task<message> UpdateMessageAsync(message message)
    {
        await _messageRepository.UpdateAsync(message);
        await _unitOfWork.CompleteAsync();
        return message;
    }

    public async Task<bool> DeleteMessageAsync(Guid id)
    {
        var result = await _messageRepository.DeleteAsync(id);
        if (result)
        {
            await _unitOfWork.CompleteAsync();
        }
        return result;
    }

    public async Task<IEnumerable<message>> GetRecentMessagesAsync(int count = 50)
    {
        var messages = await _messageRepository.GetAllAsync();
        return messages.OrderByDescending(m => m.created_at).Take(count);
    }

    public async Task<int> GetTotalMessagesCountAsync()
    {
        var messages = await _messageRepository.GetAllAsync();
        return messages.Count();
    }
}