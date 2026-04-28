using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DotnetRagApi.Application.Abstractions;
using DotnetRagApi.Application.Interfaces;
using DotnetRagApi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using ApiAskQuestion = DotnetRagApi.Application.Models.AskQuestion;
using ApiAskResponseDto = DotnetRagApi.Api.Models.AskResponseDto;

namespace DotnetRagApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AskController : ControllerBase
    {
        private readonly IAskUseCase _askUseCase;
        private readonly IIaConsultaRepository _iaConsultaRepository;
        private readonly ILogger<AskController> _logger;

        public AskController(
            IAskUseCase askUseCase,
            IIaConsultaRepository iaConsultaRepository,
            ILogger<AskController> logger)
        {
            _askUseCase = askUseCase;
            _iaConsultaRepository = iaConsultaRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiAskResponseDto>> Post([FromBody] ApiAskQuestion? request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Pregunta))
            {
                return BadRequest("La pregunta es obligatoria.");
            }

            try
            {
                _logger.LogInformation("Pregunta recibida para usuario {Usuario}", request.Usuario);

                var answer = await _askUseCase.ExecuteAsync(request, cancellationToken);

                var result = new ApiAskResponseDto
                {
                    Ok = answer.Ok,
                    Respuesta = answer.Respuesta,
                    Modelo = answer.Modelo,
                    TiempoMs = answer.TiempoMs,
                    CostoEstimado = answer.CostoEstimado,
                    ChunksUsados = answer.ChunksUsados.ToList()
                };

                await LogConsultaAsync(request, result, true, null, cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar el servicio RAG");
                await LogConsultaAsync(request, null, false, ex.Message, cancellationToken);

                return StatusCode(500, new
                {
                    ok = false,
                    message = "Ocurrio un error interno procesando la consulta."
                });
            }
        }

        private async Task LogConsultaAsync(
            ApiAskQuestion? request,
            ApiAskResponseDto? pythonResponse,
            bool ok,
            string? errorMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                var logEntry = new IaConsulta
                {
                    Usuario = request?.Usuario ?? string.Empty,
                    Rol = request?.Rol ?? string.Empty,
                    Pregunta = request?.Pregunta ?? string.Empty,
                    Respuesta = pythonResponse?.Respuesta ?? string.Empty,
                    Modelo = pythonResponse?.Modelo ?? string.Empty,
                    TiempoMs = pythonResponse?.TiempoMs ?? default,
                    CostoEstimado = pythonResponse?.CostoEstimado ?? default,
                    ChunksUsadosJson = pythonResponse == null
                        ? string.Empty
                        : JsonSerializer.Serialize(pythonResponse.ChunksUsados),
                    Ok = ok,
                    ErrorMessage = errorMessage ?? string.Empty
                };

                await _iaConsultaRepository.InsertAsync(logEntry, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando log en SQL");
            }
        }
    }
}