using ChessLogic.Pieces;

namespace ChessServer.Domain.Dtos;

public sealed class CurrentPlayingGameDto
{
    public Guid Id { get; set; }
    public string Fen { get; set; } = null!;
    public PlayerColor Color { get; set; }
    public string OpponentUsername { get; set; } = null!;
} 