using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenTalk.Models;

namespace OpenTalk.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
        };
    }

    public async Task<IReadOnlyList<ChatGroup>> GetGroupsAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<List<ChatGroup>>($"api/v1/groups/{userId}", cancellationToken)
            ?? [];
    }

    public async Task<IReadOnlyList<GroupMember>> GetGroupMembersAsync(long groupId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<List<GroupMember>>($"api/v1/groups/{groupId}/members", cancellationToken)
            ?? [];
    }

    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(long groupId, int limit = 50, long? beforeId = null, CancellationToken cancellationToken = default)
    {
        var query = beforeId.HasValue ? $"?limit={limit}&beforeId={beforeId.Value}" : $"?limit={limit}";
        return await _httpClient.GetFromJsonAsync<List<ChatMessage>>($"api/v1/messages/{groupId}{query}", cancellationToken)
            ?? [];
    }

    public async Task<IReadOnlyList<ChatMessage>> GetDirectMessagesAsync(long userId, long peerId, int limit = 50, long? beforeId = null, CancellationToken cancellationToken = default)
    {
        var query = beforeId.HasValue ? $"?limit={limit}&beforeId={beforeId.Value}" : $"?limit={limit}";
        return await _httpClient.GetFromJsonAsync<List<ChatMessage>>($"api/v1/messages/direct/{userId}/{peerId}{query}", cancellationToken)
            ?? [];
    }

    public async Task<UploadFileResult?> UploadFileAsync(string filePath, long uploaderId, CancellationToken cancellationToken = default)
    {
        await using var fileStream = File.OpenRead(filePath);
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", Path.GetFileName(filePath));
        content.Add(new StringContent(uploaderId.ToString()), "uploaderId");

        using var response = await _httpClient.PostAsync("api/v1/files/upload", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UploadFileResult>(cancellationToken: cancellationToken);
    }
}
