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
    Task<long> CreateAsync(ChatGroup group);
    Task<int> DeleteAsync(long groupId);
    Task<IEnumerable<GroupMember>> ListMembersAsync(long groupId);
    Task<int> AddMemberAsync(long groupId, long userId, DateTime joinedAt);
    Task<int> RemoveMemberAsync(long groupId, long userId);
}

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User> InsertAsync(User user);
    Task<IEnumerable<User>> ListAsync(int limit = 100, int offset = 0);
    Task<int> DeleteAsync(long id);
}
