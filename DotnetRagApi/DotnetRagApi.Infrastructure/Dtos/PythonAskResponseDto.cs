using System.Text.Json.Serialization;

namespace DotnetRagApi.Infrastructure.Dtos;

internal sealed class PythonAskResponseDto
{
    [JsonPropertyName("respuesta")]
    public string Respuesta { get; set; } = string.Empty;

    [JsonPropertyName("chunks_usados")]
    public List<string> ChunksUsados { get; set; } = new();

    [JsonPropertyName("modelo")]
    public string Modelo { get; set; } = string.Empty;

    [JsonPropertyName("costo_estimado")]
    public decimal? CostoEstimado { get; set; }

    [JsonPropertyName("tiempo_ms")]
    public int TiempoMs { get; set; }
}
