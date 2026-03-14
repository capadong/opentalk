using OpenTalk.Application.DTOs;
using OpenTalk.Domain.Entities;
using OpenTalk.Infrastructure.Repositories;

namespace OpenTalk.Application.Services;

public class MessageService(IMessageRepository messages, IGroupRepository groups, IDirectMessageRepository directMessages)
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

    public Task<IEnumerable<ChatMessage>> GetRecentAsync(long groupId, int limit = 50, long? beforeId = null)
        => messages.GetByGroupAsync(groupId, limit, beforeId);

    public async Task<DirectMessage> SaveDirectMessageAsync(SendDirectMessageDto dto)
    {
        var message = new DirectMessage
        {
            SenderId = dto.SenderId,
            ReceiverId = dto.ReceiverId,
            Type = dto.Type,
            Content = dto.Content,
            FileUrl = dto.FileUrl,
            CreatedAt = dto.CreatedAt
        };
        return await directMessages.InsertAsync(message);
    }

    public Task<IEnumerable<DirectMessage>> GetDirectRecentAsync(long userId, long peerId, int limit = 50, long? beforeId = null)
        => directMessages.GetRecentAsync(userId, peerId, limit, beforeId);
}
