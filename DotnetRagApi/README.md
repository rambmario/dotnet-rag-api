# .NET 8 RAG API

AI Q&A API built with Clean Architecture on ASP.NET Core (.NET 8).

This project is part of a portfolio-ready monorepo and demonstrates:

- clear layer separation,
- unit and integration tests,
- HTTP integration with a sibling Python RAG service,
- SQL Server persistence for request/response audit logs.

## Solution Structure

```text
DotnetRagApi/
|-- DotnetRagApi.Api
|-- DotnetRagApi.Application
|-- DotnetRagApi.Domain
|-- DotnetRagApi.Infrastructure
|-- tests
`-- README.md
```

## Architecture Layers

- DotnetRagApi.Api: HTTP endpoints, input validation, and request/response mapping.
- DotnetRagApi.Application: use cases and application contracts.
- DotnetRagApi.Domain: domain entities and core business objects.
- DotnetRagApi.Infrastructure: technical implementations (Python RAG HTTP client and SQL persistence).

## Requirements

- .NET SDK 8+
- SQL Server LocalDB (or any SQL Server instance via connection string)
- External Python RAG service (optional for unit tests, required for end-to-end API calls)

## Python RAG Service Dependency

The API expects a Python RAG service running at:

- `http://127.0.0.1:8000`

In this workspace, the RAG service is available in the sibling folder `../rag-service`.

## Run Locally

Before starting the API, create the SQL database used by the persistence layer.

Database script:

- `database/create-database.sql`

Default database name:

- `DotnetRagApiDb`

Execute that script in SQL Server Management Studio or with `sqlcmd` against LocalDB or your target SQL Server instance.

From the DotnetRagApi folder:

```powershell
dotnet restore
dotnet build
dotnet run --project .\DotnetRagApi.Api\DotnetRagApi.Api.csproj
```

Swagger is available at:

- http://localhost:5000/swagger
- https://localhost:5001/swagger

## Configuration

In DotnetRagApi.Api/appsettings.json:

- RagApi:BaseUrl
- RagApi:TimeoutSeconds
- ConnectionStrings:DefaultConnection

Quick example:


```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<YOUR_CONNECTION_STRING_HERE>"
  },
  "RagApi": {
    "BaseUrl": "http://127.0.0.1:8000",
    "TimeoutSeconds": 60
  }
}
```

Notes:

- The project does not generate the database or table on startup.
- The repository writes to `dbo.DotnetRagApiLogs`, so that table must exist before calling the API.
- If you rename the database, update the connection string accordingly.

## Testing

From the DotnetRagApi folder:

```powershell
dotnet test .\DotnetRagApi.slnx
```

Included test coverage:

- Application unit tests.
- Integration tests for /api/ask.

## Endpoints

- POST /api/ask: sends a user question and returns a response from the external RAG service.
- GET /api/logs?top=50: returns the latest persisted request logs.

## Quick Manual Test

After both services are running:

```bash
curl -X POST http://localhost:5000/api/ask \
  -H "Content-Type: application/json" \
  -d '{"pregunta":"What is retrieval augmented generation?","usuario":"recruiter","rol":"user","topN":2}'
```

## Suggested Next Improvements

- Add FluentValidation and a global exception handling middleware.
- Add health checks for SQL and the Python RAG service.
- Keep extending CI (quality gates, coverage reports, and static analysis).

## Portfolio Notes

If you are reviewing this project for a hiring process, the most relevant points are:

- clean separation between API, application, domain, and infrastructure,
- production-style integration boundaries (external AI service + SQL logging),
- testable application core with automated unit and integration testing,
- CI workflow included for build and test automation.
