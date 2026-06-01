# SafeDose AI — Infrastructure

## Docker

Local development uses Docker Compose. See `docker-compose.yml`.

```
docker-compose up -d
```

Spins up:
- SQL Server container
- Backend API container (port 5000)
- Frontend container (port 4200)

## CI/CD

GitHub Actions workflows live in `.github/workflows/`:
- `ci.yml` — runs on every push/PR (build + test)
- `deploy.yml` — runs on merge to main (deploys to cloud)

## Cloud Deployment

Target: Azure or GCP. Production secrets stored in cloud secret manager. No hardcoded keys.

## Monitoring

- Application Insights for .NET telemetry
- Langfuse for AI agent observability
