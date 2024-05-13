using ChessLogic;
using ChessLogic.Pieces;

namespace ChessServer.Domain.Models;

public enum GameResult
{
    BlackWin, WhiteWin, Draw, Playing, Aborted
}

public static class GameResultExtensions
{
    public static GameResult ToGameResult(this Result result)
    {
        if (result.Winner == PlayerColor.None)
            return GameResult.Draw;

        return result.Winner == PlayerColor.White ? GameResult.WhiteWin : GameResult.BlackWin;
    }
}