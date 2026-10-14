using DotnetRagApi.Api.OpenApi;
using DotnetRagApi.Api.Security;
using DotnetRagApi.Application;
using DotnetRagApi.Infrastructure;
using DotnetRagApi.Infrastructure.Health;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new()
        {
            Title = ".NET 10 AI RAG API",
            Version = "v1",
            Description =
                "Clean Architecture Web API that orchestrates a Python RAG service and persists SQL Server audit logs."
        };
        return Task.CompletedTask;
    });
    options.AddDocumentTransformer<ApiKeySecuritySchemeTransformer>();
});

builder.Services
    .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName,
        options =>
        {
            options.ApiKey = builder.Configuration.GetSection(ApiKeyAuthenticationOptions.SectionName)["ApiKey"]
                ?? string.Empty;
        });

builder.Services.AddAuthorization();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 60;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running."), tags: ["live"])
    .AddCheck<SqlServerHealthCheck>("sqlserver", failureStatus: HealthStatus.Unhealthy, tags: ["ready"])
    .AddCheck<RagServiceHealthCheck>("rag-service", failureStatus: HealthStatus.Unhealthy, tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar/v1", options =>
    {
        options.WithTitle(".NET 10 AI RAG API");
    });
}

if (!app.Configuration.GetValue("DisableHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiting("api");
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

public partial class Program
{
}
