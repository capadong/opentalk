using OpenTalk.Domain.Entities;
using OpenTalk.Infrastructure.Repositories;

namespace OpenTalk.Application.Services;

public record GroupMemberDto(long UserId, string Username, string? Nickname, string? Avatar, DateTime JoinedAt);

public class GroupService(IGroupRepository groups, IUserRepository users)
{
    public Task<IEnumerable<ChatGroup>> ListAsync(long userId) => groups.ListAsync(userId);

    public Task<bool> IsMemberAsync(long groupId, long userId) => groups.IsMemberAsync(groupId, userId);

    public async Task<IEnumerable<GroupMemberDto>> ListMembersAsync(long groupId)
    {
        var members = await groups.ListMembersAsync(groupId);
        var ids = members.Select(m => m.UserId).Distinct().ToArray();
        if (ids.Length == 0) return Array.Empty<GroupMemberDto>();

        var usersList = await users.GetByIdsAsync(ids);
        var dict = usersList.ToDictionary(u => u.Id);

        return members.Select(m =>
        {
            dict.TryGetValue(m.UserId, out var u);
            var username = u?.Username ?? $"用户 {m.UserId}";
            var nickname = u?.Nickname;
            var avatar = u?.Avatar;
            return new GroupMemberDto(m.UserId, username, nickname, avatar, m.JoinedAt);
        });
    }
}
