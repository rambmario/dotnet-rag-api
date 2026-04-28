using System.Data;
using Dapper;
using DotnetRagApi.Application.Interfaces;
using DotnetRagApi.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DotnetRagApi.Infrastructure.Persistence
{
    public class IaConsultaRepository : IIaConsultaRepository
    {
        private readonly string _connectionString;

        public IaConsultaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la connection string DefaultConnection.");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<long> InsertAsync(IaConsulta entity, CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO dbo.DotnetRagApiLogs
(
    Usuario,
    Rol,
    Pregunta,
    Respuesta,
    Modelo,
    TiempoMs,
    CostoEstimado,
    ChunksUsadosJson,
    Ok,
    ErrorMessage
)
VALUES
(
    @Usuario,
    @Rol,
    @Pregunta,
    @Respuesta,
    @Modelo,
    @TiempoMs,
    @CostoEstimado,
    @ChunksUsadosJson,
    @Ok,
    @ErrorMessage
);

SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

            using var connection = CreateConnection();

            var command = new CommandDefinition(sql, entity, cancellationToken: cancellationToken);
            var id = await connection.ExecuteScalarAsync<long>(command);
            return id;
        }

        public async Task<IEnumerable<IaConsulta>> GetTopAsync(int top = 50, CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT TOP (@Top)
    Id,
    Usuario,
    Rol,
    Pregunta,
    Respuesta,
    Modelo,
    TiempoMs,
    CostoEstimado,
    ChunksUsadosJson,
    Ok,
    ErrorMessage,
    FechaAlta
FROM dbo.DotnetRagApiLogs
ORDER BY Id DESC;";

            using var connection = CreateConnection();

            var command = new CommandDefinition(sql, new { Top = top }, cancellationToken: cancellationToken);
            return await connection.QueryAsync<IaConsulta>(command);
        }
    }
}