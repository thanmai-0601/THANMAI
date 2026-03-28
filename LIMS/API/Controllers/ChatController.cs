using Application.DTOs.Chat;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponseDto>> Chat([FromBody] ChatRequestDto request)
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role);
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                if (string.IsNullOrEmpty(role))
                {
                    return Unauthorized("User role not found in token.");
                }

                var response = await _chatService.GetChatResponseAsync(request.Message, role, userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Gracefully catch any API key or Groq errors so it shows inside the chat bubble
                // instead of triggering the frontend's global Server Error (500) interceptor!
                return Ok(new ChatResponseDto 
                { 
                    Response = $"Technical Error: {ex.Message}", 
                    Role = "System" 
                });
            }
        }
    }
}
