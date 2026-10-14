using System.Net;
using System.Text;
using System.Text.Json;
using DotnetRagApi.Application.Models;
using DotnetRagApi.Infrastructure.Services;

namespace DotnetRagApi.Infrastructure.Tests;

public sealed class RagClientServiceTests
{
    [Fact]
    public async Task AskAsync_MapsSuccessfulPythonResponse()
    {
        var payload = JsonSerializer.Serialize(new
        {
            respuesta = "hola",
            chunks_usados = new[] { "c1" },
            modelo = "gpt-test",
            costo_estimado = 0.01,
            tiempo_ms = 42
        });

        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        });

        var service = new RagClientService(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        });

        var result = await service.AskAsync(new AskQuestion
        {
            Pregunta = "test",
            Usuario = "mario",
            Rol = "user",
            TopN = 2
        });

        Assert.True(result.Ok);
        Assert.Equal("hola", result.Respuesta);
        Assert.Equal("gpt-test", result.Modelo);
        Assert.Equal(42, result.TiempoMs);
        Assert.Equal(0.01m, result.CostoEstimado);
        Assert.Equal(new[] { "c1" }, result.ChunksUsados);
        Assert.Equal("/rag/ask", handler.LastRequestPath);
    }

    [Fact]
    public async Task AskAsync_ThrowsHttpRequestException_WhenPythonReturnsError()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("upstream failed")
        });

        var service = new RagClientService(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        });

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.AskAsync(new AskQuestion { Pregunta = "test" }));
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StubHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public string? LastRequestPath { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestPath = request.RequestUri?.AbsolutePath;
            return Task.FromResult(_response);
        }
    }
}
