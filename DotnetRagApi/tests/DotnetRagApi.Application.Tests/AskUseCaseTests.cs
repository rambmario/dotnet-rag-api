using DotnetRagApi.Application.Abstractions;
using DotnetRagApi.Application.Interfaces;
using DotnetRagApi.Application.Models;
using DotnetRagApi.Application.UseCases;
using DotnetRagApi.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetRagApi.Application.Tests;

public sealed class AskUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ThrowsArgumentException_WhenPreguntaIsEmpty()
    {
        var ragService = new CapturingRagService();
        var repository = new CapturingRepository();
        var useCase = CreateUseCase(ragService, repository);

        var question = new AskQuestion
        {
            Pregunta = "   "
        };

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(question));
        Assert.False(ragService.WasCalled);
        Assert.Empty(repository.Inserted);
    }

    [Fact]
    public async Task ExecuteAsync_CallsRagServicePersistsLogAndReturnsResult_WhenPreguntaIsValid()
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
        var repository = new CapturingRepository();
        var useCase = CreateUseCase(ragService, repository);

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

        var log = Assert.Single(repository.Inserted);
        Assert.True(log.Ok);
        Assert.Equal("Que es RAG?", log.Pregunta);
        Assert.Equal("Respuesta de prueba", log.Respuesta);
        Assert.Equal("fake-model", log.Modelo);
        Assert.Contains("chunk-1", log.ChunksUsadosJson);
    }

    [Fact]
    public async Task ExecuteAsync_PersistsFailureLogAndRethrows_WhenRagServiceFails()
    {
        var ragService = new CapturingRagService
        {
            ExceptionToThrow = new HttpRequestException("RAG down")
        };
        var repository = new CapturingRepository();
        var useCase = CreateUseCase(ragService, repository);

        var question = new AskQuestion
        {
            Pregunta = "Que es RAG?",
            Usuario = "mario",
            Rol = "user"
        };

        await Assert.ThrowsAsync<HttpRequestException>(() => useCase.ExecuteAsync(question));

        var log = Assert.Single(repository.Inserted);
        Assert.False(log.Ok);
        Assert.Equal("Que es RAG?", log.Pregunta);
        Assert.Equal("RAG down", log.ErrorMessage);
    }

    private static AskUseCase CreateUseCase(IRagService ragService, IIaConsultaRepository repository)
    {
        return new AskUseCase(ragService, repository, NullLogger<AskUseCase>.Instance);
    }

    private sealed class CapturingRagService : IRagService
    {
        public AskQuestion? CapturedQuestion { get; private set; }
        public bool WasCalled { get; private set; }
        public Exception? ExceptionToThrow { get; set; }
        public AskAnswer NextResponse { get; set; } = new AskAnswer
        {
            Ok = true,
            Respuesta = "default"
        };

        public Task<AskAnswer> AskAsync(AskQuestion question, CancellationToken cancellationToken = default)
        {
            CapturedQuestion = question;
            WasCalled = true;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(NextResponse);
        }
    }

    private sealed class CapturingRepository : IIaConsultaRepository
    {
        public List<IaConsulta> Inserted { get; } = new();

        public Task<long> InsertAsync(IaConsulta entity, CancellationToken cancellationToken = default)
        {
            Inserted.Add(entity);
            return Task.FromResult((long)Inserted.Count);
        }

        public Task<IEnumerable<IaConsulta>> GetTopAsync(int top = 50, CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<IaConsulta>>(Array.Empty<IaConsulta>());
    }
}
