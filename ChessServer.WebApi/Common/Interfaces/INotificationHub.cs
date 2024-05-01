namespace ChessServer.WebApi.Common.Interfaces;

public interface INotificationHub
{
    Task OnConnectedAsync(string message);
    Task OnMessageReceived(string message);
}