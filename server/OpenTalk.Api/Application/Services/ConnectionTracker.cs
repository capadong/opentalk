using System.Collections.Concurrent;

namespace OpenTalk.Application.Services;

public class ConnectionTracker
{
    private readonly ConcurrentDictionary<string, long> _connections = new();

    public void Add(string connectionId, long userId)
    {
        _connections[connectionId] = userId;
    }

    public void Remove(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public int OnlineCount => _connections.Values.Distinct().Count();
}

