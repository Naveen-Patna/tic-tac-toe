using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Repositories;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController(ScoreboardRepository repository) : ControllerBase
{
    [HttpGet]
    public ActionResult<Scoreboard> Get() => Ok(repository.Get());

    [HttpPost("reset")]
    public IActionResult Reset()
    {
        repository.Reset();
        return NoContent();
    }
}
