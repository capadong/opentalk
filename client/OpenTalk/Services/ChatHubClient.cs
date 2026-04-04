using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using OpenTalk.Models;

namespace OpenTalk.Services;

public sealed class ChatHubClient : IAsyncDisposable
{
    private readonly HubConnection _connection;

    public ChatHubClient(string baseUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(baseUrl.TrimEnd('/') + "/hubs/chat")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<ChatMessage>("ReceiveMessage", message => GroupMessageReceived?.Invoke(message));
        _connection.On<ChatMessage>("ReceiveDirectMessage", message => DirectMessageReceived?.Invoke(message));
        _connection.Reconnecting += _ =>
        {
            StateChanged?.Invoke("Reconnecting");
            return Task.CompletedTask;
        };
        _connection.Reconnected += _ =>
        {
            StateChanged?.Invoke("Connected");
            return Task.CompletedTask;
        };
        _connection.Closed += _ =>
        {
            StateChanged?.Invoke("Disconnected");
            return Task.CompletedTask;
        };
    }

    public event Action<ChatMessage>? GroupMessageReceived;
    public event Action<ChatMessage>? DirectMessageReceived;
    public event Action<string>? StateChanged;

    public async Task StartAsync(long userId, CancellationToken cancellationToken = default)
    {
        StateChanged?.Invoke("Connecting");
        await _connection.StartAsync(cancellationToken);
        await _connection.InvokeAsync("Register", userId, cancellationToken);
        StateChanged?.Invoke("Connected");
    }

    public Task JoinGroupAsync(long groupId, CancellationToken cancellationToken = default)
        => _connection.InvokeAsync("JoinGroup", groupId, cancellationToken);

    public Task LeaveGroupAsync(long groupId, CancellationToken cancellationToken = default)
        => _connection.InvokeAsync("LeaveGroup", groupId, cancellationToken);

    public Task SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default)
        => _connection.InvokeAsync("SendMessage", request, cancellationToken);

    public Task SendDirectMessageAsync(SendDirectMessageRequest request, CancellationToken cancellationToken = default)
        => _connection.InvokeAsync("SendDirectMessage", request, cancellationToken);

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
