using System.Collections.Concurrent;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Repositories;

public class GameRepository
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();

    public Game Create(GameMode mode)
    {
        var game = new Game { Mode = mode };
        _games[game.Id] = game;
        return game;
    }

    public Game? Get(Guid id) =>
        _games.TryGetValue(id, out var game) ? game : null;

    public void Save(Game game) => _games[game.Id] = game;
}
