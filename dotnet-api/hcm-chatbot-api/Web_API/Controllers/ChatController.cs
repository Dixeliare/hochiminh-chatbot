using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Data;
using Models.DTOs;
using Services.Interfaces;
using System.Text.Json;

namespace Web_API;

[Authorize]
public class ChatController : BaseController
{
    private readonly IConversationService _conversationService;
    private readonly IMessageService _messageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ChatController(
        IConversationService conversationService,
        IMessageService messageService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _conversationService = conversationService;
        _messageService = messageService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Get or create conversation
            conversation? conv = null;
            if (request.ConversationId.HasValue)
            {
                conv = await _conversationService.GetByIdAsync(request.ConversationId.Value);
                if (conv == null || conv.user_id != userId)
                    return ErrorResponse("Conversation not found", 404);
            }
            else
            {
                // Create new conversation
                conv = await _conversationService.CreateAsync(new CreateConversationRequest
                {
                    Title = request.Message.Length > 50 ? request.Message.Substring(0, 50) + "..." : request.Message
                }, userId);
            }

            // Save user message
            var userMessage = await _messageService.CreateAsync(new CreateMessageRequest
            {
                ConversationId = conv.id,
                Content = request.Message,
                Role = "user"
            });

            // Call Python AI service
            var httpClient = _httpClientFactory.CreateClient();
            var aiApiUrl = _configuration["AiService:BaseUrl"] ?? "http://localhost:8000";

            var aiRequest = new { question = request.Message };
            var aiResponse = await httpClient.PostAsJsonAsync($"{aiApiUrl}/chat", aiRequest);

            if (!aiResponse.IsSuccessStatusCode)
                return ErrorResponse("AI service unavailable", 503);

            var aiResult = await aiResponse.Content.ReadFromJsonAsync<AiResponseDto>();

            // Save AI response
            var assistantMessage = await _messageService.CreateAsync(new CreateMessageRequest
            {
                ConversationId = conv.id,
                Content = aiResult?.Answer ?? "Sorry, I couldn't generate a response.",
                Role = "assistant",
                Sources = aiResult?.Sources,
                ConfidenceScore = aiResult?.Confidence
            });

            var response = new ChatApiResponse
            {
                ConversationId = conv.id,
                UserMessage = new MessageDto
                {
                    Id = userMessage.id,
                    Content = userMessage.content,
                    Role = userMessage.role,
                    CreatedAt = userMessage.created_at ?? DateTime.UtcNow
                },
                AssistantMessage = new MessageDto
                {
                    Id = assistantMessage.id,
                    Content = assistantMessage.content,
                    Role = assistantMessage.role,
                    Sources = assistantMessage.sources,
                    ConfidenceScore = assistantMessage.confidence_score ?? 0,
                    CreatedAt = assistantMessage.created_at ?? DateTime.UtcNow
                }
            };

            return SuccessResponse(response, "Message sent successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to send message: {ex.Message}", 500);
        }
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        try
        {
            var userId = GetCurrentUserId();
            var conversations = await _conversationService.GetByUserIdAsync(userId);

            var conversationDtos = conversations.Select(c => new ConversationDto
            {
                Id = c.id,
                Title = c.title,
                MessageCount = c.message_count ?? 0,
                CreatedAt = c.created_at ?? DateTime.UtcNow,
                UpdatedAt = c.updated_at ?? DateTime.UtcNow
            }).ToList();

            return SuccessResponse(conversationDtos, "Conversations retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to get conversations: {ex.Message}", 500);
        }
    }

    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(Guid conversationId)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Verify conversation belongs to user
            var conversation = await _conversationService.GetByIdAsync(conversationId);
            if (conversation == null || conversation.user_id != userId)
                return ErrorResponse("Conversation not found", 404);

            var messages = await _messageService.GetByConversationIdAsync(conversationId);

            var messageDtos = messages.Select(m => new MessageDto
            {
                Id = m.id,
                Content = m.content,
                Role = m.role,
                Sources = m.sources,
                ConfidenceScore = m.confidence_score,
                CreatedAt = m.created_at ?? DateTime.UtcNow
            }).ToList();

            return SuccessResponse(messageDtos, "Messages retrieved successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to get messages: {ex.Message}", 500);
        }
    }

    [HttpDelete("conversations/{conversationId}")]
    public async Task<IActionResult> DeleteConversation(Guid conversationId)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Verify conversation belongs to user
            var conversation = await _conversationService.GetByIdAsync(conversationId);
            if (conversation == null || conversation.user_id != userId)
                return ErrorResponse("Conversation not found", 404);

            await _conversationService.DeleteAsync(conversationId);
            return SuccessResponse<object>(null, "Conversation deleted successfully");
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Failed to delete conversation: {ex.Message}", 500);
        }
    }
}

// DTOs for chat functionality
public class SendMessageRequest
{
    public Guid? ConversationId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ChatApiResponse
{
    public Guid ConversationId { get; set; }
    public MessageDto UserMessage { get; set; } = null!;
    public MessageDto AssistantMessage { get; set; } = null!;
}

public class AiResponseDto
{
    public string Answer { get; set; } = string.Empty;
    public List<string> Sources { get; set; } = new();
    public int Confidence { get; set; }
    public string? LastUpdated { get; set; }
}