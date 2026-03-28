using Application.DTOs.Chat;

namespace Application.Interfaces.Services
{
    public interface IChatService
    {
        Task<ChatResponseDto> GetChatResponseAsync(string message, string role, int userId);
    }
}
