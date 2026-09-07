using TicTacToe.Api.Models;
using TicTacToe.Api.Repositories;

namespace TicTacToe.Api.Services;

public class GameService(GameRepository games, ScoreboardRepository scoreboard)
{
    private static readonly int[][] Lines =
    [
        [0,1,2], [3,4,5], [6,7,8],
        [0,3,6], [1,4,7], [2,5,8],
        [0,4,8], [2,4,6]
    ];

    public GameResponse Create(GameMode mode) => Response(games.Create(mode));

    public GameResponse Get(Guid id) => Response(GetGame(id));

    public GameResponse Move(Guid id, MoveRequest request)
    {
        var game = GetGame(id);
        ValidateMove(game, request);

        MakeMove(game, request.Player, request.Row, request.Column);

        if (game.Status == GameStatus.InProgress &&
            game.Mode == GameMode.Computer)
        {
            var computerMove = GetComputerMove(game);
            if (computerMove.HasValue)
                MakeMove(game, Player.O, computerMove.Value.Row, computerMove.Value.Column);
        }

        return Response(game);
    }

    public GameResponse Undo(Guid id)
    {
        var game = GetGame(id);

        if (game.Status != GameStatus.InProgress)
            throw new InvalidOperationException("Undo is disabled after the game is completed.");

        if (game.Moves.Count == 0)
            throw new InvalidOperationException("There are no moves to undo.");

        var count = game.Mode == GameMode.Computer ? 2 : 1;
        count = Math.Min(count, game.Moves.Count);

        for (var i = 0; i < count; i++)
        {
            var move = game.Moves[^1];
            game.Board[move.Row, move.Column] = null;
            game.Moves.RemoveAt(game.Moves.Count - 1);
        }

        game.CurrentPlayer = game.Moves.Count % 2 == 0 ? Player.X : Player.O;
        return Response(game);
    }

    public GameResponse Reset(Guid id)
    {
        var old = GetGame(id);
        var fresh = new Game { Id = old.Id, Mode = old.Mode };
        games.Save(fresh);
        return Response(fresh);
    }

    private Game GetGame(Guid id) =>
        games.Get(id) ?? throw new KeyNotFoundException($"Game '{id}' was not found.");

    private static void ValidateMove(Game game, MoveRequest request)
    {
        if (game.Status != GameStatus.InProgress)
            throw new InvalidOperationException("Game is already completed.");

        if (request.Row is < 0 or > 2 || request.Column is < 0 or > 2)
            throw new InvalidOperationException("Row and column must be between 0 and 2.");

        if (game.Board[request.Row, request.Column] != null)
            throw new InvalidOperationException("Cell is already occupied.");

        if (request.Player != game.CurrentPlayer)
            throw new InvalidOperationException($"It is {game.CurrentPlayer}'s turn.");

        if (game.Mode == GameMode.Computer && request.Player != Player.X)
            throw new InvalidOperationException("Only X can play in computer mode.");
    }

    private void MakeMove(Game game, Player player, int row, int column)
    {
        game.Board[row, column] = player;
        game.Moves.Add(new Move
        {
            Number = game.Moves.Count + 1,
            Player = player,
            Row = row,
            Column = column
        });

        var line = GetWinningLine(game.Board, player);
        if (line != null)
        {
            game.Status = GameStatus.Won;
            game.Winner = player;
            game.WinningCells = line.ToList();
            scoreboard.Update(game);
            return;
        }

        if (IsFull(game.Board))
        {
            game.Status = GameStatus.Draw;
            scoreboard.Update(game);
            return;
        }

        game.CurrentPlayer = player == Player.X ? Player.O : Player.X;
    }

    private static int[]? GetWinningLine(Player?[,] board, Player player)
    {
        foreach (var line in Lines)
            if (line.All(i => board[i / 3, i % 3] == player))
                return line;

        return null;
    }

    private static bool IsFull(Player?[,] board)
    {
        for (var r = 0; r < 3; r++)
            for (var c = 0; c < 3; c++)
                if (board[r, c] == null) return false;
        return true;
    }

    private static (int Row, int Column)? GetComputerMove(Game game)
    {
        // 1. Win. 2. Block. 3. Center. 4. Corner. 5. Any cell.
        var move = FindWinningMove(game, Player.O);
        if (move.HasValue) return move;

        move = FindWinningMove(game, Player.X);
        if (move.HasValue) return move;

        if (game.Board[1, 1] == null) return (1, 1);

        foreach (var corner in new[] { (0,0), (0,2), (2,0), (2,2) })
            if (game.Board[corner.Item1, corner.Item2] == null) return corner;

        for (var r = 0; r < 3; r++)
            for (var c = 0; c < 3; c++)
                if (game.Board[r, c] == null) return (r, c);

        return null;
    }

    private static (int Row, int Column)? FindWinningMove(Game game, Player player)
    {
        for (var r = 0; r < 3; r++)
        for (var c = 0; c < 3; c++)
        {
            if (game.Board[r, c] != null) continue;

            game.Board[r, c] = player;
            var wins = GetWinningLine(game.Board, player) != null;
            game.Board[r, c] = null;

            if (wins) return (r, c);
        }

        return null;
    }

    private GameResponse Response(Game game)
    {
        var board = new Player?[3][];

        for (var row = 0; row < 3; row++)
        {
            board[row] = new Player?[3];

            for (var column = 0; column < 3; column++)
            {
                board[row][column] = game.Board[row, column];
            }
        }

        return new GameResponse(
            game.Id,
            board,
            game.CurrentPlayer,
            game.Mode,
            game.Status,
            game.Winner,
            game.WinningCells,
            game.Moves,
            scoreboard.Get(),
            game.Status == GameStatus.InProgress && game.Moves.Count > 0);
    }
}
