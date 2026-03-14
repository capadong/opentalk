using Microsoft.Extensions.Configuration;

namespace OpenTalk.Application.Storage;

/// <summary>
/// 阿里云 OSS 存储占位实现（策略模式扩展点）
/// 使用时安装 Aliyun.OSS.SDK.NetCore 并实现以下方法。
/// </summary>
public class OssStorageProvider(IConfiguration configuration) : IStorageProvider
{
    // 读取 OSS 配置：Storage:Oss:Endpoint / BucketName / AccessKeyId / AccessKeySecret / PublicBaseUrl
    private readonly IConfiguration _cfg = configuration;

    public Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        // TODO: 安装 Aliyun.OSS.SDK.NetCore 后解除注释
        // var oss = new OssClient(endpoint, accessKeyId, accessKeySecret);
        // var key = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        // oss.PutObject(bucketName, key, stream);
        // return Task.FromResult($"{publicBaseUrl}/{key}");
        throw new NotImplementedException("OssStorageProvider 尚未配置，请安装 Aliyun.OSS.SDK.NetCore 并实现此方法。");
    }

    public Task<bool> DeleteAsync(string filePath, CancellationToken ct = default)
    {
        // TODO: 安装 Aliyun.OSS.SDK.NetCore 后解除注释
        // var oss = new OssClient(endpoint, accessKeyId, accessKeySecret);
        // var key = filePath.TrimStart('/');
        // oss.DeleteObject(bucketName, key);
        // return Task.FromResult(true);
        throw new NotImplementedException("OssStorageProvider 尚未配置，请安装 Aliyun.OSS.SDK.NetCore 并实现此方法。");
    }
}
