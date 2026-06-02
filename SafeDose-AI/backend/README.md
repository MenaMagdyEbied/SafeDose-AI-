# SafeDose AI Backend (.NET 10)

Clean Architecture — 5 projects.

## Structure

```
backend/
├── SafeDose.sln                     ← Solution file
├── src/
│   ├── SafeDose.Api/                ← Web API (controllers, Program.cs)
│   ├── SafeDose.Application/        ← Use cases, interfaces, DTOs
│   ├── SafeDose.Domain/             ← Entities, enums, domain logic
│   ├── SafeDose.Infrastructure/     ← SQL repos, Pinecone, Langflow clients
│   └── SafeDose.Shared/             ← Constants, cross-cutting concerns
└── tests/
    ├── SafeDose.UnitTests/
    └── SafeDose.IntegrationTests/
```

## First run

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/SafeDose.Api
```

Swagger UI available at: http://localhost:5000/swagger

## Before running

Update `src/SafeDose.Api/appsettings.json` with:
- Your SQL Server connection string
- Pinecone API key
- Langflow flow URLs (from your shared cloud Langflow workspace)

## Dependency direction (Clean Architecture rule)

```
Api → Application → Domain
 ↓        ↓
Infrastructure → Application → Domain
```

Domain has NO dependencies. Application depends only on Domain.
Infrastructure implements Application's interfaces.
Api wires everything via dependency injection.
