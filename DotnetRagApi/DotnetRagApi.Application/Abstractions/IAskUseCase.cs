using DotnetRagApi.Application.Models;
using DotnetRagApi.Domain.Entities;

namespace DotnetRagApi.Application.Abstractions;

public interface IAskUseCase
{
    Task<AskAnswer> ExecuteAsync(AskQuestion question, CancellationToken cancellationToken = default);
}
