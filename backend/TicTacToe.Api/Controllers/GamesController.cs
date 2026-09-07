using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController(GameService service) : ControllerBase
{
    [HttpPost]
    public ActionResult<GameResponse> Create(CreateGameRequest request) =>
        Ok(service.Create(request.Mode));

    [HttpGet("{id:guid}")]
    public ActionResult<GameResponse> Get(Guid id)
    {
        try { return Ok(service.Get(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost("{id:guid}/moves")]
    public ActionResult<GameResponse> Move(Guid id, MoveRequest request)
    {
        try { return Ok(service.Move(id, request)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id:guid}/undo")]
    public ActionResult<GameResponse> Undo(Guid id)
    {
        try { return Ok(service.Undo(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id:guid}/reset")]
    public ActionResult<GameResponse> Reset(Guid id)
    {
        try { return Ok(service.Reset(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
