using Dapper;
using OpenTalk.Domain.Entities;

namespace OpenTalk.Infrastructure.Repositories;

public class DapperGroupRepository(IDbConnectionFactory db) : IGroupRepository
{
    public async Task<IEnumerable<ChatGroup>> ListAsync(long userId)
    {
        const string sql = @"SELECT g.id, g.name, g.owner_id AS OwnerId, g.created_at AS CreatedAt
                             FROM chat_groups g
                             JOIN group_members gm ON gm.group_id = g.id
                             WHERE gm.user_id = @UserId
                             ORDER BY g.id DESC";
        using var conn = db.Create();
        return await conn.QueryAsync<ChatGroup>(sql, new { UserId = userId });
    }

    public async Task<bool> IsMemberAsync(long groupId, long userId)
    {
        const string sql = @"SELECT COUNT(1) FROM group_members WHERE group_id = @GroupId AND user_id = @UserId";
        using var conn = db.Create();
        var count = await conn.ExecuteScalarAsync<long>(sql, new { GroupId = groupId, UserId = userId });
        return count > 0;
    }
}
