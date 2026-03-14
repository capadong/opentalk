using Dapper;
using OpenTalk.Domain.Entities;

namespace OpenTalk.Infrastructure.Repositories;

public class DapperDirectMessageRepository(IDbConnectionFactory db) : IDirectMessageRepository
{
    public async Task<DirectMessage> InsertAsync(DirectMessage message)
    {
        using var conn = db.Create();
        const string ins = @"INSERT INTO direct_messages (sender_id, receiver_id, type, content, file_url, created_at)
                             VALUES (@SenderId, @ReceiverId, @Type, @Content, @FileUrl, @CreatedAt)";
        await conn.ExecuteAsync(ins, message);
        var id = await conn.ExecuteScalarAsync<long>("SELECT LAST_INSERT_ID();");
        message.Id = id;
        return message;
    }

    public async Task<IEnumerable<DirectMessage>> GetRecentAsync(long userId, long peerId, int limit = 50)
    {
        const string sql = @"SELECT id, sender_id AS SenderId, receiver_id AS ReceiverId, type, content, file_url AS FileUrl, created_at AS CreatedAt
                             FROM direct_messages
                             WHERE (sender_id = @UserId AND receiver_id = @PeerId)
                                OR (sender_id = @PeerId AND receiver_id = @UserId)
                             ORDER BY id DESC
                             LIMIT @Limit";
        using var conn = db.Create();
        return await conn.QueryAsync<DirectMessage>(sql, new { UserId = userId, PeerId = peerId, Limit = limit });
    }
}

