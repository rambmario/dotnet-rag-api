using DotnetRagApi.Application.Models;
using DotnetRagApi.Domain.Entities;

namespace DotnetRagApi.Application.Abstractions;

public interface IRagService
{
    Task<AskAnswer> AskAsync(AskQuestion question, CancellationToken cancellationToken = default);
}
