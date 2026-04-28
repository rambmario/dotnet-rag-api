using DotnetRagApi.Application.Abstractions;
using DotnetRagApi.Application.Interfaces;
using DotnetRagApi.Infrastructure.Options;
using DotnetRagApi.Infrastructure.Persistence;
using DotnetRagApi.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotnetRagApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RagApiOptions>(configuration.GetSection(RagApiOptions.SectionName));

        services.AddHttpClient<IRagService, RagClientService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<RagApiOptions>>().Value;

            var baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl)
                ? "http://127.0.0.1:8000"
                : options.BaseUrl;

            var timeoutSeconds = options.TimeoutSeconds <= 0 ? 60 : options.TimeoutSeconds;

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        services.AddScoped<IIaConsultaRepository, IaConsultaRepository>();

        return services;
    }
}
