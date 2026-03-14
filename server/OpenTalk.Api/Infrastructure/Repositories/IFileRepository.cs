using OpenTalk.Domain.Entities;

namespace OpenTalk.Infrastructure.Repositories;

public interface IFileRepository
{
    Task<FileRecord> InsertAsync(FileRecord record);
    Task<IEnumerable<FileRecord>> ListAsync(int limit = 100, int offset = 0);
    Task<FileRecord?> GetByIdAsync(long id);
    Task<int> DeleteAsync(long id);
}
