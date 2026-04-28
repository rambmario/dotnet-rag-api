namespace DotnetRagApi.Domain.Entities
{
    public class IaConsulta
    {
        public long Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Pregunta { get; set; } = string.Empty;
        public string? Respuesta { get; set; }
        public string? Modelo { get; set; }
        public int? TiempoMs { get; set; }
        public decimal? CostoEstimado { get; set; }
        public string? ChunksUsadosJson { get; set; }
        public bool Ok { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime FechaAlta { get; set; }
    }
}