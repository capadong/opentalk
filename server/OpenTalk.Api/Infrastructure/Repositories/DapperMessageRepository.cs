using Dapper;
using OpenTalk.Domain.Entities;

namespace OpenTalk.Infrastructure.Repositories;

public class DapperMessageRepository(IDbConnectionFactory db) : IMessageRepository
{
    public async Task<ChatMessage> InsertAsync(ChatMessage message)
    {
        const string sql = @"INSERT INTO messages (group_id, sender_id, type, content, file_url, created_at)
                             VALUES (@GroupId, @SenderId, @Type, @Content, @FileUrl, @CreatedAt);
                             SELECT LAST_INSERT_ID();";
        using var conn = db.Create();
        var id = await conn.ExecuteScalarAsync<long>(sql, message);
        message.Id = id;
        return message;
    }

    public async Task<IEnumerable<ChatMessage>> GetByGroupAsync(long groupId, int limit = 50)
    {
        const string sql = @"SELECT id, group_id AS GroupId, sender_id AS SenderId, type, content, file_url AS FileUrl, created_at AS CreatedAt
                             FROM messages WHERE group_id = @GroupId ORDER BY id DESC LIMIT @Limit";
        using var conn = db.Create();
        return await conn.QueryAsync<ChatMessage>(sql, new { GroupId = groupId, Limit = limit });
    }
}
