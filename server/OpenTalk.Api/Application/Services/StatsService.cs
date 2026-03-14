using Dapper;
using OpenTalk.Infrastructure;

namespace OpenTalk.Application.Services;

public class StatsService(IDbConnectionFactory db)
{
    public async Task<int> GetUserCountAsync()
    {
        using var conn = db.Create();
        const string sql = "SELECT COUNT(*) FROM users";
        return await conn.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetGroupCountAsync()
    {
        using var conn = db.Create();
        const string sql = "SELECT COUNT(*) FROM chat_groups";
        return await conn.ExecuteScalarAsync<int>(sql);
    }

    public async Task<long> GetTodayMessageCountAsync()
    {
        using var conn = db.Create();
        const string sql = """
            SELECT
              (SELECT COUNT(*) FROM messages WHERE DATE(created_at) = CURDATE()) +
              (SELECT COUNT(*) FROM direct_messages WHERE DATE(created_at) = CURDATE())
            """;
        return await conn.ExecuteScalarAsync<long>(sql);
    }
}

