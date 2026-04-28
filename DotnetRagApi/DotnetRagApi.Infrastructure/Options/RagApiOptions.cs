namespace DotnetRagApi.Infrastructure.Options;

public sealed class RagApiOptions
{
    public const string SectionName = "RagApi";

    public string BaseUrl { get; set; } = "http://127.0.0.1:8000";
    public int TimeoutSeconds { get; set; } = 60;
}
