# SafeDose AI — AI Agents

## Architecture

8 agent components total. All flows live in a shared Langflow workspace (cloud-hosted on DataStax Astra). Each agent has its own flow with its own API endpoint URL. The .NET backend calls each endpoint based on the user action.

## Drug Interaction Pipeline — 4 sub-agents in one flow

| # | Agent | Role |
|---|-------|------|
| 1 | Retrieval Agent | Queries Pinecone for candidate drugs |
| 2 | Patient Profile Agent | Fetches patient's current meds + conditions from SQL |
| 3 | Comparison Agent | Sends combined context to LLM, gets Level 1/2/3 draft |
| 4 | Validation Agent | Applies safety rules, validates JSON, attaches citations |

## Independent agents — 4 separate flows

| # | Agent | Role |
|---|-------|------|
| 5 | Prescription Parser (OCR) | Photo → multimodal LLM → structured drug data |
| 6 | Scheduling Agent | Prescription → reminder times (meal + spacing rules) |
| 7 | Data Sync Agent | Background job: GitHub repo → Pinecone |
| 8 | Chatbot Agent | Free-form Arabic Q&A + symptom triage |

## Folder structure

```
ai/
├── flows/         Langflow flow JSON exports (one per agent)
├── prompts/       System prompts for each agent (text files)
└── README.md
```

## Setup

1. Sign up for [DataStax Astra Langflow](https://astra.datastax.com/) — free tier
2. Create a project "SafeDose AI"
3. Invite the 5 team members as collaborators
4. Each owner creates their flow in the shared workspace
5. After each flow is built, click "Export" and save the JSON to `flows/`
6. Save the endpoint URL in a shared `.env` file used by the backend

## RAG Data Layer

- Source: [karem505/egyptian-drug-database](https://github.com/karem505/egyptian-drug-database) — 24,892 Egyptian medicines (after cleaning: ~22,500)
- Embedding model: `BAAI/bge-m3` (1024-dim, multilingual)
- Vector store: Pinecone index `safedose-drugs` (cosine similarity)
- Already loaded — see `seeding/pinecone-seed.py`

## Owners

- Drug Interaction (4 sub-agents): Mina
- Prescription Parser (OCR): Ahmed
- Scheduling: Andrew
- Data Sync + Chatbot: Fady
