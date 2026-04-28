using System.Text.Json.Serialization;

namespace DotnetRagApi.Infrastructure.Dtos;

internal sealed class PythonAskRequestDto
{
    [JsonPropertyName("pregunta")]
    public string Pregunta { get; set; } = string.Empty;

    [JsonPropertyName("usuario")]
    public string Usuario { get; set; } = "anonimo";

    [JsonPropertyName("rol")]
    public string Rol { get; set; } = "user";

    [JsonPropertyName("top_n")]
    public int TopN { get; set; } = 2;
}
