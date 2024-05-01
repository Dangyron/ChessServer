using ChessServer.WebApi.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ChessServer.WebApi.Common;

public sealed class NotificationHub : Hub<INotificationHub>
{
    private readonly Dictionary<string, string> _playerConnections = new();
    
    public override async Task OnConnectedAsync()
    {
        var playerId = Context.GetHttpContext().Request.Query["playerid"]; // Assuming user ID is set during authentication
        var connectionId = Context.ConnectionId;

        // Store the mapping of player ID to connection ID
        _playerConnections[playerId] = connectionId;
        await Clients.Caller.OnMessageReceived("You are connected successfully");
        await base.OnConnectedAsync();
    }
    
    public async Task BroadcastMessage(string message)
    {
        await Clients.Caller.OnMessageReceived(message);
    }
}