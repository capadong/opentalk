using Dapper;
using OpenTalk.Domain.Entities;

namespace OpenTalk.Infrastructure.Repositories;

public class DapperGroupRepository(IDbConnectionFactory db) : IGroupRepository
{
    public async Task<IEnumerable<ChatGroup>> ListAllAsync()
    {
        const string sql = @"SELECT id, name, owner_id AS OwnerId, created_at AS CreatedAt
                             FROM chat_groups ORDER BY id DESC";
        using var conn = db.Create();
        return await conn.QueryAsync<ChatGroup>(sql);
    }

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

    public async Task<long> CreateAsync(ChatGroup group)
    {
        using var conn = db.Create();
        const string ins = @"INSERT INTO chat_groups (name, owner_id, created_at)
                             VALUES (@Name, @OwnerId, @CreatedAt)";
        await conn.ExecuteAsync(ins, group);
        var id = await conn.ExecuteScalarAsync<long>("SELECT LAST_INSERT_ID();");
        group.Id = id;
        return id;
    }

    public async Task<int> DeleteAsync(long groupId)
    {
        using var conn = db.Create();
        conn.Open();
        using var tx = conn.BeginTransaction();
        var a = await conn.ExecuteAsync("DELETE FROM group_members WHERE group_id=@GroupId", new { GroupId = groupId }, tx);
        var b = await conn.ExecuteAsync("DELETE FROM messages WHERE group_id=@GroupId", new { GroupId = groupId }, tx);
        var c = await conn.ExecuteAsync("DELETE FROM chat_groups WHERE id=@GroupId", new { GroupId = groupId }, tx);
        tx.Commit();
        return a + b + c;
    }

    public async Task<IEnumerable<GroupMember>> ListMembersAsync(long groupId)
    {
        const string sql = @"SELECT id, group_id AS GroupId, user_id AS UserId, joined_at AS JoinedAt
                             FROM group_members WHERE group_id = @GroupId ORDER BY id DESC";
        using var conn = db.Create();
        return await conn.QueryAsync<GroupMember>(sql, new { GroupId = groupId });
    }

    public async Task<int> AddMemberAsync(long groupId, long userId, DateTime joinedAt)
    {
        const string sql = @"INSERT IGNORE INTO group_members (group_id, user_id, joined_at)
                             VALUES (@GroupId, @UserId, @JoinedAt)";
        using var conn = db.Create();
        return await conn.ExecuteAsync(sql, new { GroupId = groupId, UserId = userId, JoinedAt = joinedAt });
    }

    public async Task<int> RemoveMemberAsync(long groupId, long userId)
    {
        const string sql = @"DELETE FROM group_members WHERE group_id=@GroupId AND user_id=@UserId";
        using var conn = db.Create();
        return await conn.ExecuteAsync(sql, new { GroupId = groupId, UserId = userId });
    }
}
