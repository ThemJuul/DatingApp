﻿namespace WebApi.SignalR;

public class PresenceTracker
{
    private static readonly Dictionary<string, List<string>> _onlineUsers = [];

    public Task<bool> UserConnected(string username, string connectionId)
    {
        var isOnline = false;

        lock (_onlineUsers)
        {
            if (_onlineUsers.ContainsKey(username))
            {
                _onlineUsers[username].Add(connectionId);
            }
            else
            {
                _onlineUsers.Add(username, [connectionId]);
                isOnline = true;
            }
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string username, string connectionId)
    {
        var isOffline = false;

        lock (_onlineUsers)
        {
            if (!_onlineUsers.ContainsKey(username))
            {
                return Task.FromResult(isOffline);
            }

            _onlineUsers[username].Remove(connectionId);

            if (_onlineUsers[username].Count == 0)
            {
                _onlineUsers.Remove(username);
                isOffline = true;
            }
        }

        return Task.FromResult(isOffline);
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers;

        lock (_onlineUsers)
        {
            onlineUsers = _onlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
        }

        return Task.FromResult(onlineUsers);
    }

    public static Task<List<string>> GetConnectionsForUser(string username)
    {
        List<string> connectionIds = [];

        if (_onlineUsers.TryGetValue(username, out var connections))
        {
            lock (connections)
            {
                connectionIds = connections.ToList();
            }
        }

        return Task.FromResult(connectionIds);
    }
}
