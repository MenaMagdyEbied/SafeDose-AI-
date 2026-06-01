# SafeDose AI — Database

## Schema

SQL Server, 17 tables total. See the full ERD at the project root: `SafeDose_AI_ERD_MVP.drawio`

## Migrations

EF Core migrations are used for schema changes. From the `backend/` folder:

```
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Seed Data

- Egyptian drug catalog: pre-loaded in Pinecone (not SQL Server)
- Pricing tiers: see `seed/pricing-tiers.sql`
- Admin user: see `seed/admin-bootstrap.sql`

## Vector Database (Pinecone)

- Index name: `safedose-drugs`
- Dimension: 1024 (bge-m3)
- Metric: cosine
- Region: AWS us-east-1
- Drug count: ~22,500

Pinecone is updated by the Data Sync Agent automatically.

## Owners

- Schema: Doaa (in collaboration with Mina)
- Migrations: each feature owner adds their own
