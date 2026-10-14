using DotnetRagApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetRagApi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private const int DefaultTop = 50;
    private const int MaxTop = 100;

    private readonly IIaConsultaRepository _iaConsultaRepository;

    public LogsController(IIaConsultaRepository iaConsultaRepository)
    {
        _iaConsultaRepository = iaConsultaRepository;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(
        [FromQuery] int top = DefaultTop,
        CancellationToken cancellationToken = default)
    {
        var safeTop = Math.Clamp(top, 1, MaxTop);
        var data = await _iaConsultaRepository.GetTopAsync(safeTop, cancellationToken);
        return Ok(data);
    }
}
