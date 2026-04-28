namespace DotnetRagApi.Api.Models
{
    public class AskResponseDto
    {
        public bool Ok { get; set; }
        public string Respuesta { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int TiempoMs { get; set; }
        public decimal? CostoEstimado { get; set; }
        public List<string> ChunksUsados { get; set; } = new();
    }
}