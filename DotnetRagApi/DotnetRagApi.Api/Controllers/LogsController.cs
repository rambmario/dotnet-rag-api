using DotnetRagApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DotnetRagApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly IIaConsultaRepository _iaConsultaRepository;

    public LogsController(IIaConsultaRepository iaConsultaRepository)
    {
        _iaConsultaRepository = iaConsultaRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int top = 50, CancellationToken cancellationToken = default)
    {
        var data = await _iaConsultaRepository.GetTopAsync(top, cancellationToken);
        return Ok(data);
    }
}
