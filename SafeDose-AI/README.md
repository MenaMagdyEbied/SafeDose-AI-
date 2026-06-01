# SafeDose AI

Arabic-first Progressive Web App for chronic patients in Egypt. Tracks medications, sends smart reminders, warns about drug interactions, and lets patients show a clean medication card to any doctor.

ITI Graduation Project — Team 5.

## Core Features

- **Drug Interaction Checker** — Level 1/2/3 severity between any 2+ drugs (hero feature)
- **Add prescriptions** — manual, voice, or photo (OCR)
- **Smart reminders** — respects meal timing and drug spacing rules
- **Medication card** — printable summary with QR code
- **Clinic visit timeline** — log every visit and what was prescribed
- **In-app chatbot** — Arabic Q&A including symptom triage
- **Family accounts (paid)** — one user manages multiple patients

## Tech Stack

- **Frontend:** Angular PWA (works offline, RTL Arabic-first)
- **Backend:** .NET Core 10 Web API (Clean Architecture, Modular Monolith)
- **Relational DB:** SQL Server
- **Vector DB (RAG):** Pinecone (22,500+ Egyptian medications)
- **Embedding model:** BAAI/bge-m3 (multilingual)
- **AI orchestration:** Langflow (cloud-hosted)
- **LLM:** Provider-agnostic via OpenAI-compatible interface
- **Deployment:** Docker + cloud (Azure / GCP)

## Repository Structure

```
SafeDose-AI/
├── backend/         .NET Core 10 Web API
├── frontend/        Angular PWA
├── ai/              Langflow flows + system prompts
├── database/        SQL migrations + seed scripts
└── infrastructure/  Docker, CI/CD
```

## Getting Started

### Backend

```bash
cd backend
dotnet restore
dotnet run --project src/SafeDose.Api
```

### Frontend

```bash
cd frontend/safedose-app
npm install
npm start
```

### Local SQL Server (via Docker)

```bash
docker-compose -f infrastructure/docker-compose.yml up -d
```

## AI Agents (8 total)

**Drug Interaction pipeline (4 sub-agents in one flow):**
1. Retrieval Agent — queries Pinecone
2. Patient Profile Agent — fetches patient's current meds from SQL
3. Comparison Agent — LLM reasoning, returns Level 1/2/3
4. Validation Agent — safety rules + JSON validation

**Independent agents:**
5. Prescription Parser (OCR)
6. Scheduling Agent
7. Data Sync Agent (background — GitHub repo → Pinecone)
8. Chatbot Agent (Q&A + symptom triage)

Flow JSONs are saved in `ai/flows/`. The drug-interaction system prompt is in `ai/prompts/drug-interaction.txt`.

## License

MIT — see [LICENSE](./LICENSE)
