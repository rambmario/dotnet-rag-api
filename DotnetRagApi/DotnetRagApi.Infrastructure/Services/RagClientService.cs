using System.Net.Http.Json;
using System.Text.Json;
using DotnetRagApi.Application.Abstractions;
using DotnetRagApi.Application.Models;
using DotnetRagApi.Domain.Entities;
using DotnetRagApi.Infrastructure.Dtos;

namespace DotnetRagApi.Infrastructure.Services;

public sealed class RagClientService : IRagService
{
    private readonly HttpClient _httpClient;

    public RagClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AskAnswer> AskAsync(AskQuestion question, CancellationToken cancellationToken = default)
    {
        var pythonRequest = new PythonAskRequestDto
        {
            Pregunta = question.Pregunta,
            Usuario = question.Usuario,
            Rol = question.Rol,
            TopN = question.TopN
        };

        using var response = await _httpClient.PostAsJsonAsync("/rag/ask", pythonRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Error calling Python RAG service. Status: {(int)response.StatusCode}.");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = await response.Content.ReadFromJsonAsync<PythonAskResponseDto>(options, cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException("La respuesta del servicio Python fue nula.");
        }

        return new AskAnswer
        {
            Ok = true,
            Respuesta = result.Respuesta,
            Modelo = result.Modelo,
            TiempoMs = result.TiempoMs,
            CostoEstimado = result.CostoEstimado,
            ChunksUsados = result.ChunksUsados
        };
    }
}
