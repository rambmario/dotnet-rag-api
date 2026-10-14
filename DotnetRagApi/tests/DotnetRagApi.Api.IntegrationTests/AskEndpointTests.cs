using System.Net;
using System.Net.Http.Json;
using DotnetRagApi.Api.Models;
using DotnetRagApi.Api.Security;
using DotnetRagApi.Application.Abstractions;
using DotnetRagApi.Application.Interfaces;
using DotnetRagApi.Application.Models;
using DotnetRagApi.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotnetRagApi.Api.IntegrationTests;

public sealed class AskEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private const string TestApiKey = "test-api-key";
    private readonly HttpClient _client;

    public AskEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add(ApiKeyAuthenticationHandler.HeaderName, TestApiKey);
    }

    [Fact]
    public async Task PostAsk_ReturnsUnauthorized_WhenApiKeyIsMissing()
    {
        using var client = new TestWebApplicationFactory().CreateClient();
        var request = new AskRequestDto { Pregunta = "Que es RAG?" };

        using var response = await client.PostAsJsonAsync("/api/ask", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostAsk_ReturnsBadRequest_WhenPreguntaIsEmpty()
    {
        var request = new AskRequestDto
        {
            Pregunta = "  "
        };

        using var response = await _client.PostAsJsonAsync("/api/ask", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostAsk_ReturnsExpectedPayload_WhenPreguntaIsValid()
    {
        var request = new AskRequestDto
        {
            Pregunta = "Que es RAG?",
            Usuario = "mario",
            Rol = "admin",
            TopN = 3
        };

        using var response = await _client.PostAsJsonAsync("/api/ask", request);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<AskResponseDto>();

        Assert.NotNull(payload);
        Assert.True(payload!.Ok);
        Assert.Equal("respuesta-fake", payload.Respuesta);
        Assert.Equal("fake-model", payload.Modelo);
        Assert.Equal(21, payload.TiempoMs);
        Assert.Equal(0.001m, payload.CostoEstimado);
        Assert.Equal(new[] { "chunk-a", "chunk-b" }, payload.ChunksUsados);
    }

    [Fact]
    public async Task GetLogs_ReturnsUnauthorized_WhenApiKeyIsMissing()
    {
        using var client = new TestWebApplicationFactory().CreateClient();

        using var response = await client.GetAsync("/api/logs");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetLogs_ReturnsOk_WhenAuthorized()
    {
        using var response = await _client.GetAsync("/api/logs?top=10");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Health_ReturnsOk_WithoutApiKey()
    {
        using var client = new TestWebApplicationFactory().CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Security:ApiKey", "test-api-key");
        builder.UseSetting("DisableHttpsRedirection", "true");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=localhost;Database=DotnetRagApiDb;Trusted_Connection=True;TrustServerCertificate=True;");
        builder.UseSetting("RagApi:BaseUrl", "http://127.0.0.1:8000");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAskUseCase>();
            services.AddScoped<IAskUseCase, FakeAskUseCase>();

            services.RemoveAll<IIaConsultaRepository>();
            services.AddScoped<IIaConsultaRepository, FakeIaConsultaRepository>();
        });
    }
}

internal sealed class FakeAskUseCase : IAskUseCase
{
    public Task<AskAnswer> ExecuteAsync(AskQuestion question, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AskAnswer
        {
            Ok = true,
            Respuesta = "respuesta-fake",
            Modelo = "fake-model",
            TiempoMs = 21,
            CostoEstimado = 0.001m,
            ChunksUsados = new[] { "chunk-a", "chunk-b" }
        });
    }
}

internal sealed class FakeIaConsultaRepository : IIaConsultaRepository
{
    public Task<long> InsertAsync(IaConsulta entity, CancellationToken cancellationToken = default)
        => Task.FromResult(1L);

    public Task<IEnumerable<IaConsulta>> GetTopAsync(int top = 50, CancellationToken cancellationToken = default)
        => Task.FromResult(Enumerable.Empty<IaConsulta>());
}
