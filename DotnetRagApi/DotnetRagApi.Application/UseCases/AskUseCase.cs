using DotnetRagApi.Application.Abstractions;
using DotnetRagApi.Application.Models;
using DotnetRagApi.Domain.Entities;

namespace DotnetRagApi.Application.UseCases;

public sealed class AskUseCase : IAskUseCase
{
    private readonly IRagService _ragService;

    public AskUseCase(IRagService ragService)
    {
        _ragService = ragService;
    }

    public Task<AskAnswer> ExecuteAsync(AskQuestion question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question.Pregunta))
        {
            throw new ArgumentException("La pregunta es obligatoria.", nameof(question));
        }

        return _ragService.AskAsync(question, cancellationToken);
    }
}
