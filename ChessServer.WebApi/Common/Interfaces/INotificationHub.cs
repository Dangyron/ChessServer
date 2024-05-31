namespace ChessServer.WebApi.Common.Interfaces;

public interface INotificationHub
{
    Task OnConnected(string message);
    Task OnDisconnected(string message);
    Task OnMoveReceived(string message);
    Task OnGameStarted(string message);
    Task OnGameEnded(string message);
}