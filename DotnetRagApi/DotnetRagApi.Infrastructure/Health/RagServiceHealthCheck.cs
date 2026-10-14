using DotnetRagApi.Infrastructure.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace DotnetRagApi.Infrastructure.Health;

public sealed class RagServiceHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RagApiOptions _options;

    public RagServiceHealthCheck(IHttpClientFactory httpClientFactory, IOptions<RagApiOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("RagHealth");
            var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl)
                ? "http://127.0.0.1:8000"
                : _options.BaseUrl.TrimEnd('/');

            using var response = await client.GetAsync($"{baseUrl}/rag/health", cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Python RAG service is reachable.")
                : HealthCheckResult.Unhealthy($"Python RAG service returned {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Python RAG service is unreachable.", ex);
        }
    }
}