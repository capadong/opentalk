using Dapper;
using OpenTalk.Domain.Entities;

namespace OpenTalk.Infrastructure.Repositories;

public class DapperUserRepository(IDbConnectionFactory db) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = @"SELECT id, username, password_hash AS PasswordHash, nickname, avatar, created_at AS CreatedAt
                             FROM users WHERE username = @Username LIMIT 1";
        using var conn = db.Create();
        return await conn.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<User> InsertAsync(User user)
    {
        using var conn = db.Create();
        const string ins = @"INSERT INTO users (username, password_hash, nickname, avatar, created_at)
                             VALUES (@Username, @PasswordHash, @Nickname, @Avatar, @CreatedAt)";
        await conn.ExecuteAsync(ins, user);
        var id = await conn.ExecuteScalarAsync<long>("SELECT LAST_INSERT_ID();");
        user.Id = id;
        return user;
    }

    public async Task<IEnumerable<User>> ListAsync(int limit = 100, int offset = 0)
    {
        const string sql = @"SELECT id, username, password_hash AS PasswordHash, nickname, avatar, created_at AS CreatedAt
                             FROM users ORDER BY id DESC LIMIT @Limit OFFSET @Offset";
        using var conn = db.Create();
        return await conn.QueryAsync<User>(sql, new { Limit = limit, Offset = offset });
    }

    public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<long> ids)
    {
        const string sql = @"SELECT id, username, password_hash AS PasswordHash, nickname, avatar, created_at AS CreatedAt
                             FROM users WHERE id IN @Ids";
        using var conn = db.Create();
        return await conn.QueryAsync<User>(sql, new { Ids = ids });
    }

    public async Task<int> DeleteAsync(long id)
    {
        const string sql = @"DELETE FROM users WHERE id = @Id";
        using var conn = db.Create();
        return await conn.ExecuteAsync(sql, new { Id = id });
    }
}
