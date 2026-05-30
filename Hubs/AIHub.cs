using BloomyBE.DTOs.AI;
using BloomyBE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text;

namespace BloomyBE.Hubs
{
    [Authorize]
    public class AIHub : Hub
    {
        private readonly IAIService _aiService;
        private readonly ILogger<AIHub> _logger;

        public AIHub(IAIService aiService, ILogger<AIHub> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("AI Hub connected: {UserId}", userId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Stream AI chat response like ChatGPT
        /// </summary>
        public async Task StreamChat(AIChatRequestDto dto)
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
            {
                await Clients.Caller.SendAsync("AIError", "Bạn chưa đăng nhập.");
                return;
            }

            var userId = Guid.Parse(userIdStr);

            if (string.IsNullOrWhiteSpace(dto.Message))
            {
                await Clients.Caller.SendAsync("AIError", "Tin nhắn không được để trống.");
                return;
            }

            try
            {
                var (conversationId, assistantMessageId) = await _aiService.PrepareStreamChatAsync(userId, dto);

                await Clients.Caller.SendAsync("AIStreamStart", new
                {
                    conversationId,
                    assistantMessageId
                });

                dto.ConversationId = conversationId;
                var fullResponse = new StringBuilder();

                await foreach (var chunk in _aiService.StreamChatAsync(userId, dto, assistantMessageId))
                {
                    fullResponse.Append(chunk);
                    await Clients.Caller.SendAsync("AIStreamChunk", chunk);
                }

                await _aiService.FinalizeStreamChatAsync(
                    userId, conversationId, assistantMessageId, fullResponse.ToString());

                var (visible, _) = Helpers.AIResponseParser.SplitChatResponse(fullResponse.ToString());
                var history = await _aiService.GetHistoryAsync(userId, conversationId);
                var conv = history.FirstOrDefault();

                await Clients.Caller.SendAsync("AIStreamComplete", new
                {
                    conversationId,
                    assistantMessageId,
                    reply = visible,
                    isReadyForConcept = conv?.Status == "ReadyForConcept",
                    latestConcept = conv?.LatestConcept
                });
            }
            catch (InvalidOperationException ex)
            {
                await Clients.Caller.SendAsync("AIError", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI stream error");
                await Clients.Caller.SendAsync("AIError", "Có lỗi xảy ra. Vui lòng thử lại.");
            }
        }

        public async Task SendTypingIndicator()
        {
            await Clients.Caller.SendAsync("AITyping");
        }
    }
}
