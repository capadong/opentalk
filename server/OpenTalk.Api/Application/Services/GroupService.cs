using OpenTalk.Domain.Entities;
using OpenTalk.Infrastructure.Repositories;

namespace OpenTalk.Application.Services;

public class GroupService(IGroupRepository groups)
{
    public Task<IEnumerable<ChatGroup>> ListAsync(long userId) => groups.ListAsync(userId);
    public Task<bool> IsMemberAsync(long groupId, long userId) => groups.IsMemberAsync(groupId, userId);
}
