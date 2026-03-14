using OpenTalk.Domain.Entities;

namespace OpenTalk.Infrastructure.Repositories;

public interface IMessageRepository
{
    Task<ChatMessage> InsertAsync(ChatMessage message);
    Task<IEnumerable<ChatMessage>> GetByGroupAsync(long groupId, int limit = 50);
}

public interface IDirectMessageRepository
{
    Task<DirectMessage> InsertAsync(DirectMessage message);
    Task<IEnumerable<DirectMessage>> GetRecentAsync(long userId, long peerId, int limit = 50);
}

public interface IGroupRepository
{
    Task<IEnumerable<ChatGroup>> ListAllAsync();
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
    Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<long> ids);
    Task<int> DeleteAsync(long id);
}
