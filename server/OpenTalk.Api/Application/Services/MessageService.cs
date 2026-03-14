using OpenTalk.Application.DTOs;
using OpenTalk.Domain.Entities;
using OpenTalk.Infrastructure.Repositories;

namespace OpenTalk.Application.Services;

public class MessageService(IMessageRepository messages, IGroupRepository groups)
{
    public async Task<ChatMessage> SaveMessageAsync(SendMessageDto dto)
    {
        var message = new ChatMessage
        {
            GroupId = dto.GroupId,
            SenderId = dto.SenderId,
            Type = dto.Type,
            Content = dto.Content,
            FileUrl = dto.FileUrl,
            CreatedAt = dto.CreatedAt
        };
        return await messages.InsertAsync(message);
    }

    public Task<IEnumerable<ChatMessage>> GetRecentAsync(long groupId, int limit = 50)
        => messages.GetByGroupAsync(groupId, limit);
}
