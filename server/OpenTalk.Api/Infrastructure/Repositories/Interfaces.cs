using OpenTalk.Domain.Entities;

namespace OpenTalk.Infrastructure.Repositories;

public interface IMessageRepository
{
    Task<ChatMessage> InsertAsync(ChatMessage message);
    Task<IEnumerable<ChatMessage>> GetByGroupAsync(long groupId, int limit = 50);
}

public interface IGroupRepository
{
    Task<IEnumerable<ChatGroup>> ListAsync(long userId);
    Task<bool> IsMemberAsync(long groupId, long userId);
}

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User> InsertAsync(User user);
}
