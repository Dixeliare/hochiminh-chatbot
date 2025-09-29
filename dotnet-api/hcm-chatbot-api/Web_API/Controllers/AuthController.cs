// Import các thư viện cần thiết
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Data;
using Models.DTOs;
using Services.Interfaces;

namespace Web_API;

/// <summary>
/// AuthController - Xử lý xác thực và đăng ký người dùng
/// Endpoints: /api/auth/register, /api/auth/login
/// Tạo và validate JWT tokens
/// </summary>
public class AuthController : BaseController
{
    private readonly IAuthService _authService; // Service xử lý logic authentication

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// API đăng ký tài khoản mới
    /// Tạo user trong database và trả về JWT token
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var token = await _authService.RegisterAsync(request.Username, request.Email, request.Password, request.FullName);
            var user = await _authService.GetUserByUsernameAsync(request.Username);

            if (user == null)
                return ErrorResponse("User registration failed", 500);

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

            var response = new AuthResponse
            {
                Token = token,
                User = userDto
            };

            return SuccessResponse(response, "User registered successfully");
        }
        catch (InvalidOperationException ex)
        {
            return ErrorResponse(ex.Message, 409);
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Registration failed: {ex.Message}", 500);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _authService.LoginAsync(request.Username, request.Password);
            var user = await _authService.GetUserByUsernameAsync(request.Username);

            if (user == null)
                return ErrorResponse("Login failed", 500);

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

            var response = new AuthResponse
            {
                Token = token,
                User = userDto
            };

            return SuccessResponse(response, "Login successful");
        }
        catch (UnauthorizedAccessException ex)
        {
            return ErrorResponse(ex.Message, 401);
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Login failed: {ex.Message}", 500);
        }
    }

    [HttpPost("validate-token")]
    public async Task<IActionResult> ValidateToken([FromBody] string token)
    {
        try
        {
            var isValid = await _authService.ValidateTokenAsync(token);
            return SuccessResponse(new { IsValid = isValid }, "Token validation completed");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Token validation failed: {ex.Message}", 500);
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _authService.GetUserByUsernameAsync(User.Identity?.Name ?? "");

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

            return SuccessResponse(userDto, "User information retrieved");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to get user information: {ex.Message}", 500);
        }
    }
}