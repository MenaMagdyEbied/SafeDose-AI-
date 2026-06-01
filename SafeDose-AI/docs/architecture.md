# SafeDose AI — Architecture Overview

## High-Level System

```
[Angular PWA]  →  [.NET Core 10 API]  →  [SQL Server (PHI)]
                          ↓               [Pinecone (drug RAG)]
                   [Langflow Agents]
                          ↓
              [LLM (OpenAI-compatible)]
                          ↓
                     [Langfuse]
```

## Layers (Clean Architecture, .NET Backend)

1. **Presentation** — Angular PWA (Patient app + Admin dashboard)
2. **API** — .NET Core 10 controllers
3. **Application** — use cases, business logic
4. **Domain** — entities (Patient, Prescription, Medication, Reminder, Visit, etc.)
5. **Infrastructure** — SQL Server repos, Pinecone client, Langflow client, payment gateway

## Deployment

- Modular Monolith (single deployable .NET app)
- Docker for containerization
- GitHub Actions for CI/CD
- Secrets in cloud secret manager (no hardcoded keys)

## Data Model (ERD)

See the ERD file at the project root: `SafeDose_AI_ERD_MVP.drawio`

17 entities total. Key groupings:
- Account & Auth: Account, OTPRequest
- Subscription: PricingTier, Subscription, Payment
- Patient: Patient, Doctor, ClinicVisit
- Medications: Drug, Prescription, PatientMedication, DrugDataVersion
- Tracking: Reminder, InteractionCheck, SymptomReport
- Admin: AdminUser, AuditLog

## AI Agent Pipeline

8 agent components total. See [../ai/README.md](../ai/README.md) for the full breakdown.

### Drug Interaction Pipeline (hero feature) — 4 sub-agents
1. Retrieval Agent (Pinecone search)
2. Patient Profile Agent (SQL fetch)
3. Comparison Agent (LLM reasoning)
4. Validation Agent (safety rules + JSON validation)

### Independent agents (4)
5. Prescription Parser Agent (OCR for photo prescriptions)
6. Scheduling Agent (reminder times)
7. Data Sync Agent (GitHub repo → Pinecone, background)
8. Chatbot Agent (Arabic Q&A + symptom triage)

## Compliance

Egyptian Data Protection Law (Law 151 / 2020):
- Phone-OTP authentication
- Explicit consent flow at signup
- Right to delete account (soft delete + 90-day retention)
- MENA-region data residency
- Audit log for all PHI access
