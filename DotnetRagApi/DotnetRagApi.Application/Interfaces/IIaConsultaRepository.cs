using DotnetRagApi.Domain.Entities;

namespace DotnetRagApi.Application.Interfaces
{
    public interface IIaConsultaRepository
    {
        Task<long> InsertAsync(IaConsulta entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<IaConsulta>> GetTopAsync(int top = 50, CancellationToken cancellationToken = default);
    }
}