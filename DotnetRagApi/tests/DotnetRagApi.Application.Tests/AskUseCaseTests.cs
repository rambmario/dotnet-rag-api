using DotnetRagApi.Application.Abstractions;
using DotnetRagApi.Application.Models;
using DotnetRagApi.Application.UseCases;
using DotnetRagApi.Domain.Entities;

namespace DotnetRagApi.Application.Tests;

public sealed class AskUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ThrowsArgumentException_WhenPreguntaIsEmpty()
    {
        var ragService = new CapturingRagService();
        var useCase = new AskUseCase(ragService);

        var question = new AskQuestion
        {
            Pregunta = "   "
        };

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(question));
        Assert.False(ragService.WasCalled);
    }

    [Fact]
    public async Task ExecuteAsync_CallsRagServiceAndReturnsResult_WhenPreguntaIsValid()
    {
        var expected = new AskAnswer
        {
            Ok = true,
            Respuesta = "Respuesta de prueba",
            Modelo = "fake-model",
            TiempoMs = 12,
            CostoEstimado = 0.0001m,
            ChunksUsados = new[] { "chunk-1" }
        };

        var ragService = new CapturingRagService
        {
            NextResponse = expected
        };

        var useCase = new AskUseCase(ragService);

        var question = new AskQuestion
        {
            Pregunta = "Que es RAG?",
            Usuario = "mario",
            Rol = "user",
            TopN = 2
        };

        var result = await useCase.ExecuteAsync(question);

        Assert.Same(expected, result);
        Assert.True(ragService.WasCalled);
        Assert.NotNull(ragService.CapturedQuestion);
        Assert.Equal("Que es RAG?", ragService.CapturedQuestion!.Pregunta);
        Assert.Equal("mario", ragService.CapturedQuestion.Usuario);
        Assert.Equal("user", ragService.CapturedQuestion.Rol);
        Assert.Equal(2, ragService.CapturedQuestion.TopN);
    }

    private sealed class CapturingRagService : IRagService
    {
        public AskQuestion? CapturedQuestion { get; private set; }
        public bool WasCalled { get; private set; }
        public AskAnswer NextResponse { get; set; } = new AskAnswer
        {
            Ok = true,
            Respuesta = "default"
        };

        public Task<AskAnswer> AskAsync(AskQuestion question, CancellationToken cancellationToken = default)
        {
            CapturedQuestion = question;
            WasCalled = true;
            return Task.FromResult(NextResponse);
        }
    }
}
