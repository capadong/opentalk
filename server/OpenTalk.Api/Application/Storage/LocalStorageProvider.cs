using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace OpenTalk.Application.Storage;

/// <summary>
/// 本地磁盘存储实现
/// </summary>
public class LocalStorageProvider(IConfiguration configuration, IWebHostEnvironment env) : IStorageProvider
{
    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var root = configuration.GetSection("Storage")["UploadsRoot"] ?? "wwwroot/uploads";
        var now = DateTime.UtcNow;
        var relPath = $"{now:yyyy/MM}";
        var dir = Path.Combine(env.ContentRootPath, root, relPath);
        Directory.CreateDirectory(dir);
        var fname = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var full = Path.Combine(dir, fname);
        await using var fs = new FileStream(full, FileMode.Create);
        await stream.CopyToAsync(fs, ct);
        return $"/uploads/{relPath.Replace("\\\\", "/")}/{fname}";
    }

    public Task<bool> DeleteAsync(string filePath, CancellationToken ct = default)
    {
        // filePath 形如 /uploads/2025/03/xxx.pdf，映射到物理路径
        var root = configuration.GetSection("Storage")["UploadsRoot"] ?? "wwwroot/uploads";
        // strip leading /uploads/
        var rel = filePath.TrimStart('/').Replace("uploads/", "", StringComparison.OrdinalIgnoreCase);
        var full = Path.Combine(env.ContentRootPath, root, rel.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(full))
        {
            File.Delete(full);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
