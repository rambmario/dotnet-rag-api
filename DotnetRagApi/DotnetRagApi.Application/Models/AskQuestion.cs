namespace DotnetRagApi.Application.Models;

public sealed class AskQuestion
{
    public string Pregunta { get; set; } = string.Empty;
    public string Usuario { get; set; } = "anonimo";
    public string Rol { get; set; } = "user";
    public int TopN { get; set; } = 2;
}
