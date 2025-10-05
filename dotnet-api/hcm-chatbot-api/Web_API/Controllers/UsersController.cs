using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Data;
using Models.DTOs;
using Services.Interfaces;

namespace Web_API;

/// <summary>
/// UsersController - Quản lý users, upload avatar, update profile
/// </summary>

[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly ICloudinaryService _cloudinaryService;

    public UsersController(IUserService userService, ICloudinaryService cloudinaryService)
    {
        _userService = userService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.id,
                Username = u.username,
                Email = u.email,
                FullName = u.full_name,
                AvatarUrl = u.avatar_url,
                Role = u.role ?? "user",
                Status = u.status ?? "enable",
                TotalMessages = u.total_messages ?? 0,
                TotalConversations = u.total_conversations ?? 0,
                CreatedAt = u.created_at ?? DateTime.UtcNow
            }).ToList();

            return SuccessResponse(userDtos, "Users retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to retrieve users: {ex.Message}", 500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsAdmin();

            if (id != currentUserId && !isAdmin)
                return ErrorResponse("Access denied", 403);

            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
                return ErrorResponse("User not found", 404);

            var userDto = new UserDto
            {
                Id = user.id,
                Username = user.username,
                Email = user.email,
                FullName = user.full_name,
                AvatarUrl = user.avatar_url,
                Role = user.role ?? "user",
                Status = user.status ?? "enable",
                TotalMessages = user.total_messages ?? 0,
                TotalConversations = user.total_conversations ?? 0,
                CreatedAt = user.created_at ?? DateTime.UtcNow
            };

            return SuccessResponse(userDto, "User retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to retrieve user: {ex.Message}", 500);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsAdmin();

            if (id != currentUserId && !isAdmin)
                return ErrorResponse("Access denied", 403);

            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
                return ErrorResponse("User not found", 404);

            user.full_name = request.FullName;
            user.avatar_url = request.AvatarUrl;

            if (isAdmin && !string.IsNullOrEmpty(request.Role))
            {
                user.role = request.Role;
            }

            if (isAdmin && !string.IsNullOrEmpty(request.Status))
            {
                user.status = request.Status;
            }

            var updatedUser = await _userService.UpdateUserAsync(user);

            var userDto = new UserDto
            {
                Id = updatedUser.id,
                Username = updatedUser.username,
                Email = updatedUser.email,
                FullName = updatedUser.full_name,
                AvatarUrl = updatedUser.avatar_url,
                Role = updatedUser.role ?? "user",
                Status = updatedUser.status ?? "enable",
                TotalMessages = updatedUser.total_messages ?? 0,
                TotalConversations = updatedUser.total_conversations ?? 0,
                CreatedAt = updatedUser.created_at ?? DateTime.UtcNow
            };

            return SuccessResponse(userDto, "User updated successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to update user: {ex.Message}", 500);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            if (id == currentUserId)
                return ErrorResponse("Cannot delete your own account", 400);

            var deleted = await _userService.DeleteUserAsync(id);

            if (!deleted)
                return ErrorResponse("User not found or failed to delete", 404);

            return SuccessResponse("User deleted successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to delete user: {ex.Message}", 500);
        }
    }

    [HttpPost("{id}/toggle-status")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ToggleUserStatus(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            if (id == currentUserId)
                return ErrorResponse("Cannot change your own status", 400);

            var success = await _userService.ToggleUserStatusAsync(id);

            if (!success)
                return ErrorResponse("User not found or failed to update status", 404);

            var user = await _userService.GetUserByIdAsync(id);
            return SuccessResponse($"User status changed to {user?.status}", "User status updated successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to toggle user status: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Upload avatar cho user lên Cloudinary
    /// Chấp nhận file ảnh: jpg, jpeg, png, gif, webp (max 5MB)
    /// </summary>
    [HttpPost("{id}/upload-avatar")]
    public async Task<IActionResult> UploadAvatar(Guid id, IFormFile avatar)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsAdmin();

            // Chỉ cho phép user tự upload hoặc admin
            if (id != currentUserId && !isAdmin)
                return ErrorResponse("Access denied", 403);

            // Validate file
            if (avatar == null || avatar.Length == 0)
                return ErrorResponse("No file uploaded", 400);

            // Get user
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return ErrorResponse("User not found", 404);

            // Upload lên Cloudinary
            var cloudinaryUrl = await _cloudinaryService.UploadImageAsync(avatar, "hcm-chatbot/avatars");

            // Xóa avatar cũ trên Cloudinary (nếu có)
            if (!string.IsNullOrEmpty(user.avatar_url) && user.avatar_url.Contains("cloudinary.com"))
            {
                var oldPublicId = _cloudinaryService.GetPublicIdFromUrl(user.avatar_url);
                await _cloudinaryService.DeleteImageAsync(oldPublicId);
            }

            // Update user avatar_url
            user.avatar_url = cloudinaryUrl;
            await _userService.UpdateUserAsync(user);

            var response = new UploadAvatarResponse { AvatarUrl = cloudinaryUrl };
            return SuccessResponse(response, "Avatar uploaded successfully to Cloudinary");
        }
        catch (ArgumentException ex)
        {
            return ErrorResponse(ex.Message, 400);
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to upload avatar: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Xóa avatar của user trên Cloudinary
    /// </summary>
    [HttpDelete("{id}/avatar")]
    public async Task<IActionResult> DeleteAvatar(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsAdmin();

            if (id != currentUserId && !isAdmin)
                return ErrorResponse("Access denied", 403);

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return ErrorResponse("User not found", 404);

            // Xóa avatar trên Cloudinary
            if (!string.IsNullOrEmpty(user.avatar_url) && user.avatar_url.Contains("cloudinary.com"))
            {
                var publicId = _cloudinaryService.GetPublicIdFromUrl(user.avatar_url);
                await _cloudinaryService.DeleteImageAsync(publicId);
            }

            user.avatar_url = null;
            await _userService.UpdateUserAsync(user);

            return SuccessResponse<object>(null, "Avatar deleted successfully from Cloudinary");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to delete avatar: {ex.Message}", 500);
        }
    }

    [HttpGet("role/{role}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetUsersByRole(string role)
    {
        try
        {
            var users = await _userService.GetUsersByRoleAsync(role);

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.id,
                Username = u.username,
                Email = u.email,
                FullName = u.full_name,
                AvatarUrl = u.avatar_url,
                Role = u.role ?? "user",
                Status = u.status ?? "enable",
                TotalMessages = u.total_messages ?? 0,
                TotalConversations = u.total_conversations ?? 0,
                CreatedAt = u.created_at ?? DateTime.UtcNow
            }).ToList();

            return SuccessResponse(userDtos, $"Users with role '{role}' retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to retrieve users by role: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Đổi mật khẩu cho user
    /// </summary>
    [HttpPut("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Chỉ cho phép user tự đổi mật khẩu của mình
            if (id != currentUserId)
                return ErrorResponse("Access denied", 403);

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return ErrorResponse("User not found", 404);

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.password_hash))
                return ErrorResponse("Current password is incorrect", 400);

            // Update password
            user.password_hash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userService.UpdateUserAsync(user);

            return SuccessResponse<object>(null, "Password changed successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to change password: {ex.Message}", 500);
        }
    }
}