using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Models.DTOs;
using Services.Interfaces;

namespace Web_API;

[Authorize(Roles = "admin")]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;
    private readonly IUserService _userService;
    private readonly IConversationService _conversationService;
    private readonly IMessageService _messageService;

    public DashboardController(
        IDashboardService dashboardService,
        IUserService userService,
        IConversationService conversationService,
        IMessageService messageService)
    {
        _dashboardService = dashboardService;
        _userService = userService;
        _conversationService = conversationService;
        _messageService = messageService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();

            var dashboardStats = new DashboardStatsDto
            {
                TotalUsers = stats.TotalUsers,
                ActiveUsers = stats.ActiveUsers,
                NewUsersToday = stats.NewUsersToday,
                TotalConversations = stats.TotalConversations,
                TotalMessages = stats.TotalMessages,
                MessagesToday = stats.MessagesToday,
                ConversationsToday = stats.ConversationsToday,
                RecentStats = stats.RecentStats.Select(s => new DailyStatDto
                {
                    Id = s.id,
                    Date = s.date,
                    TotalUsers = s.total_users ?? 0,
                    NewUsers = s.new_users ?? 0,
                    ActiveUsers = s.active_users ?? 0,
                    TotalMessages = s.total_messages ?? 0,
                    TotalConversations = s.total_conversations ?? 0,
                    CreatedAt = s.created_at ?? DateTime.UtcNow
                }).ToList()
            };

            return SuccessResponse(dashboardStats, "Dashboard stats retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to retrieve dashboard stats: {ex.Message}", 500);
        }
    }

    [HttpGet("users/stats")]
    public async Task<IActionResult> GetUserStats()
    {
        try
        {
            var allUsers = await _userService.GetAllUsersAsync();
            var usersList = allUsers.ToList();

            var userStats = new UserStatsDto
            {
                TotalCount = usersList.Count,
                ActiveCount = usersList.Count(u => u.status == "enable"),
                DisabledCount = usersList.Count(u => u.status == "disable"),
                AdminCount = usersList.Count(u => u.role == "admin"),
                NewToday = usersList.Count(u => u.created_at?.Date == DateTime.Today),
                RecentUsers = usersList
                    .OrderByDescending(u => u.created_at)
                    .Take(10)
                    .Select(u => new UserDto
                    {
                        Id = u.id,
                        Username = u.username,
                        Email = u.email,
                        FullName = u.full_name,
                        Role = u.role ?? "user",
                        Status = u.status ?? "enable",
                        TotalMessages = u.total_messages ?? 0,
                        TotalConversations = u.total_conversations ?? 0,
                        CreatedAt = u.created_at ?? DateTime.UtcNow
                    }).ToList()
            };

            return SuccessResponse(userStats, "User stats retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to retrieve user stats: {ex.Message}", 500);
        }
    }

    [HttpGet("conversations/stats")]
    public async Task<IActionResult> GetConversationStats()
    {
        try
        {
            var recentConversations = await _conversationService.GetRecentConversationsAsync(10);
            var conversationsList = recentConversations.ToList();

            var today = DateTime.Today;
            var thisWeek = DateTime.Today.AddDays(-7);
            var thisMonth = DateTime.Today.AddDays(-30);

            var conversationStats = new ConversationStatsDto
            {
                TotalCount = conversationsList.Count,
                TodayCount = conversationsList.Count(c => c.created_at?.Date == today),
                ThisWeekCount = conversationsList.Count(c => c.created_at >= thisWeek),
                ThisMonthCount = conversationsList.Count(c => c.created_at >= thisMonth),
                RecentConversations = conversationsList.Select(c => new ConversationSummaryDto
                {
                    Id = c.id,
                    Title = c.title,
                    MessageCount = c.message_count ?? 0,
                    CreatedAt = c.created_at ?? DateTime.UtcNow,
                    UpdatedAt = c.updated_at ?? DateTime.UtcNow
                }).ToList()
            };

            return SuccessResponse(conversationStats, "Conversation stats retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to retrieve conversation stats: {ex.Message}", 500);
        }
    }

    [HttpGet("messages/stats")]
    public async Task<IActionResult> GetMessageStats()
    {
        try
        {
            var totalMessages = await _messageService.GetTotalMessagesCountAsync();
            var recentMessages = await _messageService.GetRecentMessagesAsync(100);
            var messagesList = recentMessages.ToList();

            var today = DateTime.Today;
            var thisWeek = DateTime.Today.AddDays(-7);
            var thisMonth = DateTime.Today.AddDays(-30);

            var totalConversations = await _conversationService.GetRecentConversationsAsync(1000);
            var conversationCount = totalConversations.Count();

            var messageStats = new MessageStatsDto
            {
                TotalCount = totalMessages,
                TodayCount = messagesList.Count(m => m.created_at?.Date == today),
                ThisWeekCount = messagesList.Count(m => m.created_at >= thisWeek),
                ThisMonthCount = messagesList.Count(m => m.created_at >= thisMonth),
                AveragePerConversation = conversationCount > 0 ? (double)totalMessages / conversationCount : 0
            };

            return SuccessResponse(messageStats, "Message stats retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to retrieve message stats: {ex.Message}", 500);
        }
    }

    [HttpPost("update-daily-stats")]
    public async Task<IActionResult> UpdateDailyStats()
    {
        try
        {
            await _dashboardService.UpdateDailyStatsAsync();
            return SuccessResponse("Daily stats updated successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to update daily stats: {ex.Message}", 500);
        }
    }

    [HttpGet("stats/date-range")]
    public async Task<IActionResult> GetStatsForDateRange([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
    {
        try
        {
            var stats = await _dashboardService.GetStatsForDateRangeAsync(fromDate, toDate);

            var statsDto = stats.Select(s => new DailyStatDto
            {
                Id = s.id,
                Date = s.date,
                TotalUsers = s.total_users ?? 0,
                NewUsers = s.new_users ?? 0,
                ActiveUsers = s.active_users ?? 0,
                TotalMessages = s.total_messages ?? 0,
                TotalConversations = s.total_conversations ?? 0,
                CreatedAt = s.created_at ?? DateTime.UtcNow
            }).ToList();

            return SuccessResponse(statsDto, "Stats for date range retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to retrieve stats for date range: {ex.Message}", 500);
        }
    }
}