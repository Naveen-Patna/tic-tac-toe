using System;
using Xunit;
using TicTacToe.Api.Models;
using TicTacToe.Api.Repositories;
using TicTacToe.Api.Services;

namespace TicTacToe.Tests;

public class GameServiceTests
{
    private static GameService CreateService() => new(new GameRepository(), new ScoreboardRepository());

    [Fact]
    public void Create_StartsWithX()
    {
        var service = CreateService();
        var game = service.Create(GameMode.TwoPlayer);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Empty(game.MoveHistory);
    }

    [Fact]
    public void ValidMove_ChangesTurnAndAddsHistory()
    {
        var service = CreateService();
        var game = service.Create(GameMode.TwoPlayer);
        var result = service.Move(game.Id, new MoveRequest(Player.X, 0, 0));
        Assert.Equal(Player.O, result.CurrentPlayer);
        Assert.Equal(Player.X, result.Board[0][0]);
        Assert.Single(result.MoveHistory);
    }

    [Fact]
    public void InvalidMove_DoesNotChangeTurn()
    {
        var service = CreateService();
        var game = service.Create(GameMode.TwoPlayer);
        service.Move(game.Id, new MoveRequest(Player.X, 0, 0));
        Assert.Throws<InvalidOperationException>(() => service.Move(game.Id, new MoveRequest(Player.X, 0, 1)));
        var result = service.Get(game.Id);
        Assert.Equal(Player.O, result.CurrentPlayer);
        Assert.Single(result.MoveHistory);
    }

    [Fact]
    public void OccupiedCell_IsRejected()
    {
        var service = CreateService();
        var game = service.Create(GameMode.TwoPlayer);
        service.Move(game.Id, new MoveRequest(Player.X, 0, 0));
        Assert.Throws<InvalidOperationException>(() => service.Move(game.Id, new MoveRequest(Player.O, 0, 0)));
    }

    [Fact]
    public void RowWin_IsDetected()
    {
        var service = CreateService();
        var game = service.Create(GameMode.TwoPlayer);
        service.Move(game.Id, new MoveRequest(Player.X, 0, 0));
        service.Move(game.Id, new MoveRequest(Player.O, 1, 0));
        service.Move(game.Id, new MoveRequest(Player.X, 0, 1));
        service.Move(game.Id, new MoveRequest(Player.O, 1, 1));
        var result = service.Move(game.Id, new MoveRequest(Player.X, 0, 2));
        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new[] { 0, 1, 2 }, result.WinningCells);
        Assert.Equal(1, result.Scoreboard.XWins);
    }

    [Fact]
    public void ColumnWin_IsDetected()
    {
        var service = CreateService();
        var game = service.Create(GameMode.TwoPlayer);
        service.Move(game.Id, new MoveRequest(Player.X, 0, 0));
        service.Move(game.Id, new MoveRequest(Player.O, 0, 1));
        service.Move(game.Id, new MoveRequest(Player.X, 1, 0));
        service.Move(game.Id, new MoveRequest(Player.O, 1, 1));
        var result = service.Move(game.Id, new MoveRequest(Player.X, 2, 0));
        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new[] { 0, 3, 6 }, result.WinningCells);
    }

    [Fact]
    public void DiagonalWin_IsDetected()
    {
        var service = CreateService();
        var game = service.Create(GameMode.TwoPlayer);
        service.Move(game.Id, new MoveRequest(Player.X, 0, 0));
        service.Move(game.Id, new MoveRequest(Player.O, 0, 1));
        service.Move(game.Id, new MoveRequest(Player.X, 1, 1));
        service.Move(game.Id, new MoveRequest(Player.O, 0, 2));
        var result = service.Move(game.Id, new MoveRequest(Player.X, 2, 2));
        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new[] { 0, 4, 8 }, result.WinningCells);
    }

    [Fact]
    public void Draw_IsDetected()
    {
        var service = CreateService();
        var game = service.Create(GameMode.TwoPlayer);
        var moves = new[]
        {
            new MoveRequest(Player.X,0,0), new MoveRequest(Player.O,0,1), new MoveRequest(Player.X,0,2),
            new MoveRequest(Player.O,1,1), new MoveRequest(Player.X,1,0), new MoveRequest(Player.O,1,2),
            new MoveRequest(Player.X,2,1), new MoveRequest(Player.O,2,0), new MoveRequest(Player.X,2,2)
        };
        GameResponse? result = null;
        foreach (var move in moves) result = service.Move(game.Id, move);
        Assert.NotNull(result);
        Assert.Equal(GameStatus.Draw, result!.Status);
        Assert.Null(result.Winner);
        Assert.Equal(1, result.Scoreboard.Draws);
    }

    [Fact]
    public void MoveAfterCompletion_IsRejected()
    {
        var service = CreateService();
        var game = service.Create(GameMode.TwoPlayer);
        service.Move(game.Id, new MoveRequest(Player.X,0,0));
        service.Move(game.Id, new MoveRequest(Player.O,1,0));
        service.Move(game.Id, new MoveRequest(Player.X,0,1));
        service.Move(game.Id, new MoveRequest(Player.O,1,1));
        service.Move(game.Id, new MoveRequest(Player.X,0,2));
        Assert.Throws<InvalidOperationException>(() => service.Move(game.Id, new MoveRequest(Player.O,2,2)));
    }

    [Fact]
    public void Undo_TwoPlayer_RemovesOneMoveAndRestoresTurn()
    {
        var service = CreateService();
        var game = service.Create(GameMode.TwoPlayer);
        service.Move(game.Id, new MoveRequest(Player.X,0,0));
        service.Move(game.Id, new MoveRequest(Player.O,1,1));
        var result = service.Undo(game.Id);
        Assert.Single(result.MoveHistory);
        Assert.Null(result.Board[1][1]);
        Assert.Equal(Player.O, result.CurrentPlayer);
    }

    [Fact]
    public void Undo_Computer_RemovesHumanAndComputerMoves()
    {
        var service = CreateService();
        var game = service.Create(GameMode.Computer);
        var afterMove = service.Move(game.Id, new MoveRequest(Player.X,0,0));
        Assert.Equal(2, afterMove.MoveHistory.Count);
        var result = service.Undo(game.Id);
        Assert.Empty(result.MoveHistory);
        Assert.Null(result.Board[0][0]);
        Assert.Equal(Player.X, result.CurrentPlayer);
        Assert.False(result.CanUndo);
    }

    [Fact]
    public void Reset_ClearsGameButKeepsScoreboard()
    {
        var service = CreateService();
        var game = service.Create(GameMode.TwoPlayer);
        service.Move(game.Id, new MoveRequest(Player.X,0,0));
        service.Move(game.Id, new MoveRequest(Player.O,1,0));
        service.Move(game.Id, new MoveRequest(Player.X,0,1));
        service.Move(game.Id, new MoveRequest(Player.O,1,1));
        service.Move(game.Id, new MoveRequest(Player.X,0,2));
        var result = service.Reset(game.Id);
        Assert.Empty(result.MoveHistory);
        Assert.Equal(Player.X, result.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, result.Status);
        Assert.Equal(1, result.Scoreboard.XWins);
        Assert.Null(result.Board[0][0]);
    }

    [Fact]
    public void Computer_MakesAutomaticMove()
    {
        var service = CreateService();
        var game = service.Create(GameMode.Computer);
        var result = service.Move(game.Id, new MoveRequest(Player.X,0,0));
        Assert.Equal(2, result.MoveHistory.Count);
        Assert.Equal(Player.X, result.MoveHistory[0].Player);
        Assert.Equal(Player.O, result.MoveHistory[1].Player);
        Assert.Equal(Player.X, result.CurrentPlayer);
    }

    [Fact]
    public void Computer_BlocksHumanWinningMove()
    {
        var service = CreateService();
        var game = service.Create(GameMode.Computer);
        service.Move(game.Id, new MoveRequest(Player.X,0,0));
        service.Move(game.Id, new MoveRequest(Player.X,0,1));
        var result = service.Get(game.Id);
        Assert.Equal(Player.O, result.Board[0][2]);
        Assert.Equal(GameStatus.InProgress, result.Status);
    }

    [Fact]
    public void Scoreboard_IsUpdatedOnlyOnce()
    {
        var service = CreateService();
        var game = service.Create(GameMode.TwoPlayer);
        service.Move(game.Id, new MoveRequest(Player.X,0,0));
        service.Move(game.Id, new MoveRequest(Player.O,1,0));
        service.Move(game.Id, new MoveRequest(Player.X,0,1));
        service.Move(game.Id, new MoveRequest(Player.O,1,1));
        var result = service.Move(game.Id, new MoveRequest(Player.X,0,2));
        Assert.Equal(1, result.Scoreboard.XWins);
        Assert.Equal(0, result.Scoreboard.OWins);
        Assert.Equal(0, result.Scoreboard.Draws);
    }
}
