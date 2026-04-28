using DotnetRagApi.Application;
using DotnetRagApi.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = ".NET 8 AI RAG API",
		Version = "v1",
		Description = "Clean Architecture Web API that orchestrates a Python RAG service and persists SQL Server audit logs."
	});
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
	options.DocumentTitle = ".NET 8 AI RAG API";
	options.SwaggerEndpoint("/swagger/v1/swagger.json", ".NET 8 AI RAG API v1");
});

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}