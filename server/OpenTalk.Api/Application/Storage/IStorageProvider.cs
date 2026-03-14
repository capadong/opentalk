namespace OpenTalk.Application.Storage;

/// <summary>
/// 存储提供者接口 - 策略模式，支持本地存储、OSS 等多种后端
/// </summary>
public interface IStorageProvider
{
    /// <summary>上传文件，返回可访问的相对/绝对 URL</summary>
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);

    /// <summary>删除文件（path 为 UploadAsync 返回的 URL/路径）</summary>
    Task<bool> DeleteAsync(string filePath, CancellationToken ct = default);
}
