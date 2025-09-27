using Microsoft.EntityFrameworkCore;
using Data;
using Repositories;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implementations;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<user> _userRepository;

    public UserService(IUnitOfWork unitOfWork, IGenericRepository<user> userRepository)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<user>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<user?> GetUserByIdAsync(Guid id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<user> CreateUserAsync(user user)
    {
        user.id = Guid.NewGuid();
        user.created_at = DateTime.UtcNow;
        user.updated_at = DateTime.UtcNow;
        user.total_messages ??= 0;
        user.total_conversations ??= 0;
        user.role ??= "user";
        user.status ??= "enable";

        var result = await _userRepository.AddAsync(user);
        await _unitOfWork.CompleteAsync();
        return result;
    }

    public async Task<user> UpdateUserAsync(user user)
    {
        user.updated_at = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CompleteAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var result = await _userRepository.DeleteAsync(id);
        if (result)
        {
            await _unitOfWork.CompleteAsync();
        }
        return result;
    }

    public async Task<bool> ToggleUserStatusAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        user.status = user.status == "enable" ? "disable" : "enable";
        user.updated_at = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<IEnumerable<user>> GetUsersByRoleAsync(string role)
    {
        var users = await _userRepository.GetAllAsync();
        return users.Where(u => u.role == role);
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Count();
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Count(u => u.status == "enable");
    }

    public async Task UpdateUserStatsAsync(Guid userId, int messageIncrement = 0, int conversationIncrement = 0)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return;

        user.total_messages = (user.total_messages ?? 0) + messageIncrement;
        user.total_conversations = (user.total_conversations ?? 0) + conversationIncrement;
        user.updated_at = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.CompleteAsync();
    }
}