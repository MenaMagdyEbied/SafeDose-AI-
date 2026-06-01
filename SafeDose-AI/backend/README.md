# SafeDose AI — Backend (.NET Core 10)

## Architecture

Clean Architecture with 5 layers (will be created as separate projects in the .NET solution):

```
backend/src/
├── SafeDose.Api/             Presentation layer — controllers, DTOs, middleware
├── SafeDose.Application/     Use cases, application services
├── SafeDose.Domain/          Entities, domain logic
├── SafeDose.Infrastructure/  SQL Server, Pinecone client, Langflow client, external APIs
└── SafeDose.Shared/          Cross-cutting concerns
```

## Setup (after a teammate clones)

1. Install [.NET 10 SDK](https://dotnet.microsoft.com/download)
2. Install SQL Server (LocalDB or Docker)
3. Copy `.env.example` to `.env` and fill in:
   - `SQL_CONNECTION_STRING`
   - `PINECONE_API_KEY`
   - `LANGFLOW_BASE_URL`
   - `LANGFLOW_API_KEY`
   - `JWT_SECRET`
   - `PAYMENT_GATEWAY_KEY`
4. Run migrations: `dotnet ef database update`
5. Run: `dotnet run --project src/SafeDose.Api`

## API Documentation

Swagger UI is exposed at `/swagger` when running locally.

See [../docs/api-contracts.md](../docs/api-contracts.md) for the full API specification.

## Tests

```
dotnet test
```

## Owners

- Lead: Doaa (Auth + Account + Subscription)
- Integration: Mina
