using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using ChessServer.WebApi.Common.Extensions;
using ChessServer.WebApi.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ChessServer.WebApi.Common;

public sealed class NotificationHub : Hub<INotificationHub>
{
    private readonly ConcurrentDictionary<Guid, string> _playerConnections;

    public NotificationHub(ConcurrentDictionary<Guid, string> playerConnections)
    {
        _playerConnections = playerConnections;
    }

    public override async Task OnConnectedAsync()
    {
        var token = Context.GetHttpContext()!.Request.Query["token"];
        var claims = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;

        var playerId = claims!.GetId();

        _playerConnections[playerId] = Context.ConnectionId;
        await Clients.Caller.OnConnected("You are connected successfully");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var key = _playerConnections.First(pair => pair.Value == Context.ConnectionId).Key;
        _playerConnections.TryRemove(key, out _);
        await Clients.Caller.OnDisconnected("You are disconnected successfully");

        await base.OnDisconnectedAsync(exception);
    }
}