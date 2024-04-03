using ChessServer.Data.Repositories.Interfaces;
using ChessServer.Domain.DtoS;
using ChessServer.WebApi.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChessServer.WebApi.Controllers;

[Route("game")]
public class GameController : BaseController
{
    private readonly IGameRepository _gameRepository;

    public GameController(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    [HttpPost("move")]
    public async Task<IActionResult> Move([FromBody]GameDto gameDto)
    {
        var currGame = await _gameRepository.GetAsync(g => g.Id == gameDto.Id);

        if (currGame is null)
            throw new Exception();

        return Ok(currGame);
    }
}