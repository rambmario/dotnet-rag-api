# .NET 10 RAG API

AI Q&A API built with Clean Architecture on ASP.NET Core (.NET 10).

This project is part of a portfolio-ready monorepo and demonstrates:

- clear layer separation,
- use-case orchestration with SQL audit logging,
- API key authentication and health checks,
- resilient HTTP integration with a sibling Python RAG service,
- unit and integration tests.

## Solution Structure

```text
DotnetRagApi/
|-- DotnetRagApi.Api
|-- DotnetRagApi.Application
|-- DotnetRagApi.Domain
|-- DotnetRagApi.Infrastructure
|-- database
|-- tests
`-- README.md
```

## Architecture Layers

- **Api**: HTTP endpoints, DTO mapping, auth, OpenAPI/Scalar, rate limiting.
- **Application**: use cases and application contracts (`AskUseCase` orchestrates RAG + audit persistence).
- **Domain**: domain entities and core business objects.
- **Infrastructure**: Python RAG HTTP client (resilience handler), SQL persistence, health checks.

## Requirements

- .NET SDK 10+
- SQL Server (LocalDB, full SQL Server, or Docker)
- External Python RAG service (required for end-to-end calls)

## Run with Docker

From the repository root:

```powershell
docker compose up --build
```

API docs: `http://localhost:5000/scalar/v1`

## Run Locally

1. Create the database with `database/create-database.sql`.
2. Copy `DotnetRagApi.Api/appsettings.example.json` to `appsettings.json`.
3. Set connection string, `Security:ApiKey`, and `RagApi:BaseUrl`.

```powershell
dotnet restore
dotnet build
dotnet run --project .\DotnetRagApi.Api\DotnetRagApi.Api.csproj
```

Scalar (Development):

- http://localhost:5000/scalar/v1
- https://localhost:5001/scalar/v1

## Configuration

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server audit database |
| `Security:ApiKey` | Required `X-Api-Key` header value |
| `RagApi:BaseUrl` | Python service base URL |
| `RagApi:TimeoutSeconds` | HTTP timeout |
| `DisableHttpsRedirection` | Useful in Docker/HTTP-only hosting |

## Testing

```powershell
dotnet test .\DotnetRagApi.slnx
```

Coverage includes:

- Application unit tests (success, validation, failure logging).
- Infrastructure client mapping tests.
- API integration tests (auth, validation, happy path, health).

## Endpoints

- `POST /api/ask` (API key)
- `GET /api/logs?top=50` (API key, `top` clamped to 1..100)
- `GET /health`
- `GET /health/ready`

## Quick Manual Test

```bash
curl -X POST http://localhost:5000/api/ask \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: change-me-demo-api-key" \
  -d '{"pregunta":"What is retrieval augmented generation?","usuario":"demo","rol":"user","topN":2}'
```

## Portfolio Notes

If you are reviewing this project for a hiring process, the most relevant points are:

- clean separation between API, application, domain, and infrastructure,
- audit logging inside the application use case (not the controller),
- production-minded edges: auth, health checks, rate limiting, resilient HttpClient,
- automated unit/integration tests and CI.
