using TicTacToe.Api.Models;

namespace TicTacToe.Api.Repositories;

public class ScoreboardRepository
{
    private int _xWins;
    private int _oWins;
    private int _draws;

    public Scoreboard Get() => new(_xWins, _oWins, _draws);

    public void Update(Game game)
    {
        if (game.ScoreUpdated) return;

        if (game.Status == GameStatus.Won && game.Winner == Player.X) _xWins++;
        else if (game.Status == GameStatus.Won && game.Winner == Player.O) _oWins++;
        else if (game.Status == GameStatus.Draw) _draws++;

        game.ScoreUpdated = true;
    }

    public void Reset()
    {
        _xWins = 0;
        _oWins = 0;
        _draws = 0;
    }
}
