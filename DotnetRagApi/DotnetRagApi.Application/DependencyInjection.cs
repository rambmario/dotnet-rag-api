using DotnetRagApi.Application.Abstractions;
using DotnetRagApi.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetRagApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAskUseCase, AskUseCase>();
        return services;
    }
}
