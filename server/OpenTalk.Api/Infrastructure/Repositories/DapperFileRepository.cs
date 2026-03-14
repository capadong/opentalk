using Dapper;
using OpenTalk.Domain.Entities;

namespace OpenTalk.Infrastructure.Repositories;

public class DapperFileRepository(IDbConnectionFactory db) : IFileRepository
{
    public async Task<FileRecord> InsertAsync(FileRecord record)
    {
        const string sql = @"
            INSERT INTO files (file_name, file_path, file_type, size, uploader_id, created_at)
            VALUES (@FileName, @FilePath, @FileType, @Size, @UploaderId, @CreatedAt)";
        using var conn = db.Create();
        await conn.ExecuteAsync(sql, record);
        record.Id = await conn.ExecuteScalarAsync<long>("SELECT LAST_INSERT_ID();");
        return record;
    }

    public async Task<IEnumerable<FileRecord>> ListAsync(int limit = 100, int offset = 0)
    {
        const string sql = @"
            SELECT f.id, f.file_name AS FileName, f.file_path AS FilePath,
                   f.file_type AS FileType, f.size, f.uploader_id AS UploaderId,
                   u.username AS UploaderName, f.created_at AS CreatedAt
            FROM files f
            LEFT JOIN users u ON u.id = f.uploader_id
            ORDER BY f.id DESC
            LIMIT @Limit OFFSET @Offset";
        using var conn = db.Create();
        return await conn.QueryAsync<FileRecord>(sql, new { Limit = limit, Offset = offset });
    }

    public async Task<FileRecord?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT f.id, f.file_name AS FileName, f.file_path AS FilePath,
                   f.file_type AS FileType, f.size, f.uploader_id AS UploaderId,
                   u.username AS UploaderName, f.created_at AS CreatedAt
            FROM files f
            LEFT JOIN users u ON u.id = f.uploader_id
            WHERE f.id = @Id";
        using var conn = db.Create();
        return await conn.QuerySingleOrDefaultAsync<FileRecord>(sql, new { Id = id });
    }

    public async Task<int> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM files WHERE id = @Id";
        using var conn = db.Create();
        return await conn.ExecuteAsync(sql, new { Id = id });
    }
}
