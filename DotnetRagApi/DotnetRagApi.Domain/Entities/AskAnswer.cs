namespace DotnetRagApi.Domain.Entities;

public sealed class AskAnswer
{
    public bool Ok { get; init; }
    public string Respuesta { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public int TiempoMs { get; init; }
    public decimal? CostoEstimado { get; init; }

   
    public IReadOnlyCollection<string> ChunksUsados { get; init; } = Array.Empty<string>();
}
