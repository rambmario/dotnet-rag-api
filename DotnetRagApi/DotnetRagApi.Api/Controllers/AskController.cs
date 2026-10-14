using DotnetRagApi.Api.Models;
using DotnetRagApi.Application.Abstractions;
using DotnetRagApi.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetRagApi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AskController : ControllerBase
{
    private readonly IAskUseCase _askUseCase;
    private readonly ILogger<AskController> _logger;

    public AskController(IAskUseCase askUseCase, ILogger<AskController> logger)
    {
        _askUseCase = askUseCase;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AskResponseDto>> Post(
        [FromBody] AskRequestDto? request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Pregunta))
        {
            return Problem(
                detail: "La pregunta es obligatoria.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid request");
        }

        var question = new AskQuestion
        {
            Pregunta = request.Pregunta,
            Usuario = request.Usuario,
            Rol = request.Rol,
            TopN = request.TopN
        };

        try
        {
            _logger.LogInformation("Pregunta recibida para usuario {Usuario}", question.Usuario);

            var answer = await _askUseCase.ExecuteAsync(question, cancellationToken);

            return Ok(new AskResponseDto
            {
                Ok = answer.Ok,
                Respuesta = answer.Respuesta,
                Modelo = answer.Modelo,
                TiempoMs = answer.TiempoMs,
                CostoEstimado = answer.CostoEstimado,
                ChunksUsados = answer.ChunksUsados.ToList()
            });
        }
        catch (ArgumentException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid request");
        }
    }
}
