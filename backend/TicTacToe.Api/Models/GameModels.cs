namespace TicTacToe.Api.Models;

public enum Player { X, O }
public enum GameMode { TwoPlayer, Computer }
public enum GameStatus { InProgress, Won, Draw }

public class Move
{
    public int Number { get; set; }
    public Player Player { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
}

public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Player?[,] Board { get; } = new Player?[3, 3];
    public Player CurrentPlayer { get; set; } = Player.X;
    public GameMode Mode { get; set; }
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public Player? Winner { get; set; }
    public List<int> WinningCells { get; set; } = new List<int>();
    public List<Move> Moves { get; set; } = new List<Move>();
    public bool ScoreUpdated { get; set; }
}

public record CreateGameRequest(GameMode Mode);
public record MoveRequest(Player Player, int Row, int Column);
public record Scoreboard(int XWins, int OWins, int Draws);

public record GameResponse(
    Guid Id,
    Player?[][] Board,
    Player CurrentPlayer,
    GameMode Mode,
    GameStatus Status,
    Player? Winner,
    List<int> WinningCells,
    List<Move> MoveHistory,
    Scoreboard Scoreboard,
    bool CanUndo);
