using System.Text.Json;
using DotnetRagApi.Application.Abstractions;
using DotnetRagApi.Application.Interfaces;
using DotnetRagApi.Application.Models;
using DotnetRagApi.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DotnetRagApi.Application.UseCases;

public sealed class AskUseCase : IAskUseCase
{
    private const int MaxErrorMessageLength = 500;

    private readonly IRagService _ragService;
    private readonly IIaConsultaRepository _iaConsultaRepository;
    private readonly ILogger<AskUseCase> _logger;

    public AskUseCase(
        IRagService ragService,
        IIaConsultaRepository iaConsultaRepository,
        ILogger<AskUseCase> logger)
    {
        _ragService = ragService;
        _iaConsultaRepository = iaConsultaRepository;
        _logger = logger;
    }

    public async Task<AskAnswer> ExecuteAsync(AskQuestion question, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(question);

        if (string.IsNullOrWhiteSpace(question.Pregunta))
        {
            throw new ArgumentException("La pregunta es obligatoria.", nameof(question));
        }

        try
        {
            var answer = await _ragService.AskAsync(question, cancellationToken);
            await PersistLogAsync(question, answer, ok: true, errorMessage: null, cancellationToken);
            return answer;
        }
        catch (Exception ex) when (ex is not ArgumentException and not OperationCanceledException)
        {
            _logger.LogError(ex, "Error al consultar el servicio RAG para usuario {Usuario}", question.Usuario);
            await PersistLogAsync(question, answer: null, ok: false, errorMessage: SanitizeError(ex.Message), cancellationToken);
            throw;
        }
    }

    private async Task PersistLogAsync(
        AskQuestion question,
        AskAnswer? answer,
        bool ok,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var logEntry = new IaConsulta
            {
                Usuario = question.Usuario ?? string.Empty,
                Rol = question.Rol ?? string.Empty,
                Pregunta = question.Pregunta ?? string.Empty,
                Respuesta = answer?.Respuesta ?? string.Empty,
                Modelo = answer?.Modelo ?? string.Empty,
                TiempoMs = answer?.TiempoMs,
                CostoEstimado = answer?.CostoEstimado,
                ChunksUsadosJson = answer is null
                    ? string.Empty
                    : JsonSerializer.Serialize(answer.ChunksUsados),
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

    private static string SanitizeError(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Unexpected error";
        }

        var trimmed = message.Trim();
        return trimmed.Length <= MaxErrorMessageLength
            ? trimmed
            : trimmed[..MaxErrorMessageLength];
    }
}
