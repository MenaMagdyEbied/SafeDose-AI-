# Module 5 — Drug Interaction Checker — Full Requirements Specification

> **Module Owner:** Mina Magdy Ebied
> **Module Status:** Stages 1+3 done in Langflow, Stages 2+4 pending, .NET integration pending
> **Last Updated:** 2026-06-06
> **Document Version:** 1.0

---

## How to use this document

This is the SINGLE SOURCE OF TRUTH for Module 5. Every requirement has a unique ID (FR-001, NFR-001, UC-001, etc.). When someone asks "should the system do X?", look up the ID. If it's not in this doc, it's not in scope. If you want to add it, update this doc FIRST, then build it.

---

# PART 1 — MODULE OVERVIEW

## 1.1 What is Module 5

Module 5 is the **Drug Interaction Checker** — SafeDose's hero feature. It checks whether a new medication will conflict with:
- The patient's existing active medications
- The patient's chronic conditions
- The patient's known allergies

And returns a clear Arabic verdict at one of three safety levels (Green/Amber/Red) with sources and a recommended action.

## 1.2 Why this module exists

Chronic patients in Egypt often take 5-10 medications across multiple doctors. Doctors rarely have time to verify cross-prescriptions. Pharmacists may not have full patient history. The patient becomes the last line of defense — but they don't have the medical knowledge.

SafeDose fills this gap. Whenever the patient adds a drug, we check it instantly against their full medical picture and warn them in plain Arabic.

## 1.3 Why this matters for the demo

This is THE feature that distinguishes SafeDose from a calendar app. When the supervisor sees a Level 3 (Red) warning fire correctly for a real-world drug combination, they understand WHY we built this.

## 1.4 Scope

**IN scope for this module:**
- Adding a new drug triggers an interaction check
- Manual two-drug standalone check (no patient context)
- Display of Level 1/2/3 verdicts in Arabic with sources
- Saving check results for history
- Background re-check when medication list changes

**OUT of scope for this module:**
- Dose recommendations (we don't suggest doses)
- Treatment recommendations (we don't say "take X instead")
- Diagnosis (we never diagnose conditions)
- Emergency response actions (handled by Module 7)
- Prescription editing (handled by Module 3/4)

---

# PART 2 — BUSINESS REQUIREMENTS

## 2.1 Business Goals

| Goal ID | Goal | Success Measure |
|---------|------|-----------------|
| BG-001 | Prevent dangerous drug-drug interactions in chronic patients | ≥ 90% of major interactions in test set correctly flagged at Level 3 |
| BG-002 | Reduce confusion when adding new prescriptions | Average time from "add drug" to "see verdict" < 8 seconds |
| BG-003 | Never silently substitute or hide a medication | Zero auto-substitutions; every uncertain match requires patient confirmation |
| BG-004 | Build patient trust through transparency | Every verdict shows the SOURCE (which database/rule fired the warning) |
| BG-005 | Comply with Egyptian Data Protection Law 151/2020 | All checks logged with patient consent record |

## 2.2 Stakeholders

| Stakeholder | Their interest |
|-------------|----------------|
| **Primary user — chronic patient (40+, Arabic speaker)** | Safe medication management; clear warnings they can act on |
| **Secondary user — family caregiver** | Same info as primary, multi-patient view |
| **Doctor (read-only view)** | Patient hands them the medication card showing the check history |
| **Pharmacist (external)** | Reviews edge cases flagged for human review |
| **SafeDose team** | Auditable system for compliance |
| **Supervisor / ITI / Demo audience** | Working demo with realistic interactions |

## 2.3 Why we built it THIS way (key architectural decisions)

- **4-stage agent pipeline** instead of a single LLM call: each stage can be tested and improved independently
- **Pinecone for retrieval, LLM for reasoning**: pure LLM can hallucinate drug interactions; Pinecone grounds it in real Egyptian drug data
- **3-level severity (not 5 or 10)**: easy for elderly Arabic speakers to understand at a glance
- **Always require source citation**: builds patient trust, supports doctor reviews
- **Conservative defaults**: when uncertain, return Level 2 (amber) rather than Level 1 (green). Safety bias.

---

# PART 3 — USERS & USER STORIES

## 3.1 User personas

**Persona 1 — Uncle Mahmoud, 62, hypertension + diabetes**
- Takes 6 medications daily
- Visits 3 different doctors
- Reads Arabic; basic smartphone literacy
- Gets new prescriptions every 3 months
- Worried about taking the wrong combination

**Persona 2 — Aya, 35, daughter of Uncle Mahmoud**
- Manages her father's medications remotely
- Uses Family Account (paid)
- Wants notifications if a new drug conflicts

**Persona 3 — Dr. Khaled, internal medicine**
- Doesn't use the app himself
- Receives medication card from patient at visit
- Trusts SafeDose enough to use the card as reference

## 3.2 User stories

| Story ID | Story | Priority |
|----------|-------|----------|
| US-001 | As a patient, when I add a new drug, I want to immediately see if it's safe with my current meds | P0 (must) |
| US-002 | As a patient, I want the verdict in plain Arabic, not medical jargon | P0 |
| US-003 | As a patient, I want to know WHICH of my current drugs is the problem | P0 |
| US-004 | As a patient, I want to know what action to take (consult doctor, take separately, avoid) | P0 |
| US-005 | As a patient, I want to see the source so I know this isn't made up | P1 |
| US-006 | As a patient, I want to see my past interaction checks in history | P1 |
| US-007 | As a family member, I want to check interactions for any patient in my family account | P1 |
| US-008 | As a doctor (via medication card), I want to see flagged interactions for context | P2 |
| US-009 | As a patient, I want to do a quick standalone two-drug check before going to pharmacy | P2 |
| US-010 | As a patient with a chronic condition, I want the system to also warn me about condition-drug conflicts | P0 |
| US-011 | As a patient with allergies, I want the system to flag drugs containing my allergens | P0 |

---

# PART 4 — FUNCTIONAL REQUIREMENTS

## 4.1 Input handling (FR-100 series)

| ID | Requirement |
|----|-------------|
| FR-101 | The system SHALL accept a `drugId` (from SQL Drug table) as input for verification |
| FR-102 | The system SHALL ALSO accept a free-text `drugName` for cases where the drug isn't in the SQL DB |
| FR-103 | The system SHALL accept a `patientId` to fetch full medical context |
| FR-104 | The system SHALL support a "standalone" two-drug check that takes two `drugId` values WITHOUT patient context |
| FR-105 | The system SHALL reject requests where neither `drugId` nor `drugName` is provided (HTTP 400) |
| FR-106 | The system SHALL reject requests where the `patientId` doesn't belong to the authenticated account (HTTP 403) |

## 4.2 Pipeline execution (FR-200 series)

| ID | Requirement |
|----|-------------|
| FR-201 | The system SHALL execute the 4-stage agent pipeline in order: Retrieval → Patient Profile → Comparison → Validation |
| FR-202 | The Retrieval Agent SHALL query Pinecone for top 10 candidate drugs related to the input drug name |
| FR-203 | The Patient Profile Agent SHALL fetch from SQL: patient's active medications, chronic conditions, allergies, age, weight (if recorded) |
| FR-204 | The Comparison Agent SHALL receive both retrieval output AND patient profile, and produce a draft Level 1/2/3 verdict |
| FR-205 | The Validation Agent SHALL apply safety rules and format the final JSON output |
| FR-206 | If any agent stage fails or times out (> 10 seconds), the system SHALL return a precautionary Level 2 verdict with explanation |
| FR-207 | The system SHALL cache identical requests for 1 hour (drug + patient combo) to avoid redundant LLM calls |

## 4.3 Safety rules — hard-coded checks (FR-300 series)

These checks run BEFORE the LLM pipeline, as fail-fast safety guards:

| ID | Requirement |
|----|-------------|
| FR-301 | The system SHALL automatically return Level 3 (Red) if the new drug's scientific name matches any of the patient's recorded allergies (case-insensitive, supports cross-reactivity table) |
| FR-302 | The system SHALL automatically return Level 3 if the patient is pregnant AND the drug is in Category D or X |
| FR-303 | The system SHALL automatically return Level 2 if the drug's scientific name is NOT in Pinecone (cannot verify) |
| FR-304 | The system SHALL NEVER return Level 1 (Green) if the new drug is in a known critical-pair table with any of the patient's current meds (e.g., Warfarin + Aspirin → always Red) |
| FR-305 | The system SHALL NEVER recommend dose adjustments |
| FR-306 | The system SHALL NEVER recommend stopping a medication |
| FR-307 | The system SHALL NEVER diagnose a condition based on drug interactions |
| FR-308 | Every verdict SHALL include the standard disclaimer: "استشر طبيبك أو الصيدلي" |

## 4.4 Output formatting (FR-400 series)

| ID | Requirement |
|----|-------------|
| FR-401 | The response SHALL include `level` (1, 2, or 3) |
| FR-402 | The response SHALL include `labelArabic` ("آمن" / "احذر" / "خطر") and `labelEnglish` for compatibility |
| FR-403 | The response SHALL include `color` (hex code: #4CAF50, #FFA000, #D32F2F) |
| FR-404 | The response SHALL include `explanationArabic` — 1-3 sentence plain Arabic explanation |
| FR-405 | The response SHALL include `recommendedActionArabic` — specific patient action |
| FR-406 | The response SHALL include `conflictingDrugs` — array of patient's existing drugs that triggered the warning (with their IDs and names) |
| FR-407 | The response SHALL include `conflictingConditions` — array of chronic conditions that triggered the warning |
| FR-408 | The response SHALL include `sources` — array of citations (drug database references, rule IDs) |
| FR-409 | The response SHALL include `disclaimerArabic` (the standard one) |
| FR-410 | The response SHALL include `checkedAt` timestamp (UTC) |
| FR-411 | The response SHALL include `interactionCheckId` for history tracking |

## 4.5 Persistence (FR-500 series)

| ID | Requirement |
|----|-------------|
| FR-501 | The system SHALL save every check result as an `InteractionCheck` record in SQL |
| FR-502 | The saved record SHALL include: PatientId, NewDrugId, ResultLevel, ExplanationArabic, ConflictingDrugsJson, SourcesJson, CheckedAt |
| FR-503 | The system SHALL preserve check history even if the medication is later deleted (for audit) |
| FR-504 | The system SHALL allow patient to view their last 50 checks via `/api/interactions/history` |

## 4.6 History and review (FR-600 series)

| ID | Requirement |
|----|-------------|
| FR-601 | The system SHALL provide an endpoint to list past checks filtered by patient and date range |
| FR-602 | The system SHALL provide an endpoint to fetch a single past check's full details |
| FR-603 | The system SHALL re-verify any Level 2/3 result when the patient's medication list changes (background job) |
| FR-604 | If re-verification changes the result level, the system SHALL notify the patient |

## 4.7 Integration with other modules (FR-700 series)

| ID | Requirement |
|----|-------------|
| FR-701 | Module 3 (Prescription Parser) SHALL call this module's check endpoint for each confirmed new medication |
| FR-702 | Module 4 (Medication Management) SHALL call this module's check endpoint before saving a manually-added medication |
| FR-703 | The system SHALL expose a `/check-conflicts` preview endpoint that runs the check without saving the medication |
| FR-704 | This module SHALL expose an internal endpoint for the Langflow Patient Profile Agent to fetch SQL data |

---

# PART 5 — NON-FUNCTIONAL REQUIREMENTS

## 5.1 Performance (NFR-100)

| ID | Requirement | Target |
|----|-------------|--------|
| NFR-101 | End-to-end response time for full pipeline | ≤ 8 seconds (95th percentile) |
| NFR-102 | Standalone two-drug check (no patient context) | ≤ 4 seconds |
| NFR-103 | History list endpoint | ≤ 500ms |
| NFR-104 | Cached request (warm) | ≤ 200ms |
| NFR-105 | System SHALL handle 100 concurrent check requests | with degradation < 50% on response time |

## 5.2 Reliability (NFR-200)

| ID | Requirement |
|----|-------------|
| NFR-201 | Pipeline SHALL gracefully degrade: if Stage 2 (Patient Profile) fails, return result based on Stages 1+3+4 only with a warning |
| NFR-202 | Pipeline SHALL retry Pinecone calls up to 3 times with exponential backoff (200ms, 600ms, 1.8s) |
| NFR-203 | If LLM (Gemini/Fireworks) is down, SHALL fall back to the critical-pair lookup table for known dangerous combinations |
| NFR-204 | All check operations SHALL be idempotent — calling twice with same input produces same result |

## 5.3 Security (NFR-300)

| ID | Requirement |
|----|-------------|
| NFR-301 | All endpoints (except internal) SHALL require valid JWT |
| NFR-302 | A patient SHALL ONLY see/check medications for patients in their own Account |
| NFR-303 | The internal Patient Profile endpoint SHALL use a separate Service Token (not patient JWT) |
| NFR-304 | All check results SHALL be encrypted at rest (SQL TDE) |
| NFR-305 | API keys for Pinecone/Gemini/Fireworks SHALL NEVER be returned in responses |
| NFR-306 | The system SHALL log every check with patient ID + timestamp + result level for audit (Egyptian Data Protection Law 151/2020) |

## 5.4 Accessibility & Localization (NFR-400)

| ID | Requirement |
|----|-------------|
| NFR-401 | Primary language for all user-facing strings: Arabic |
| NFR-402 | English fallback available for compatibility, never as the primary display |
| NFR-403 | Arabic text SHALL use modern standard Arabic (not classical, not heavy dialect) |
| NFR-404 | No medical jargon — explanations at grade 6 reading level |
| NFR-405 | All severity colors SHALL also have icons (for colorblind users) |
| NFR-406 | All severity labels SHALL be readable from arm's length (font size ≥ 24px) |

## 5.5 Auditability & Compliance (NFR-500)

| ID | Requirement |
|----|-------------|
| NFR-501 | Every check result SHALL include the AI model version used (e.g., "gemini-2.5-flash-2026-05") |
| NFR-502 | Every check result SHALL include the Pinecone index version used |
| NFR-503 | Every check result SHALL be linkable to the patient's consent record (Module 1) |
| NFR-504 | Deleted check results SHALL be soft-deleted (preserved for 7 years per Egyptian medical record law) |

---

# PART 6 — USE CASES

## UC-001 — Patient adds a drug from a prescription photo

**Actor:** Patient
**Preconditions:** Patient is logged in, has consent, has profile with chronic conditions/allergies set
**Trigger:** Patient confirms a medication in the Prescription Parser review screen (Module 3)

**Main flow:**
1. Module 3 calls `POST /api/interactions/check` with `patientId` and `newDrugId`
2. System fetches patient profile + active medications via Patient Profile Agent
3. System retrieves candidate drugs via Pinecone
4. System runs Comparison + Validation agents
5. System saves `InteractionCheck` record
6. System returns verdict to Module 3
7. Module 3 displays verdict on confirmation screen (color, message, action)
8. If Level 3, patient must explicitly confirm "Save anyway" with "I will consult my doctor"

**Alternate flows:**
- 1a. Drug not in SQL → use `drugName` text → Retrieval Agent searches Pinecone by name
- 4a. LLM timeout → fall back to critical-pair lookup table

**Postconditions:**
- Verdict displayed
- InteractionCheck saved
- If patient confirms despite Red, save medication with "user_acknowledged_warning" flag

## UC-002 — Patient does a standalone two-drug check

**Actor:** Patient
**Preconditions:** Logged in
**Trigger:** Patient taps "Quick Check" on home screen

**Main flow:**
1. Patient selects Drug A and Drug B from autocomplete
2. App calls `POST /api/interactions/check` with both `drugId` fields, NO patient context
3. System runs ONLY Stages 1 + 3 + 4 (skips Patient Profile)
4. System returns verdict
5. App displays verdict

**Postcondition:** No InteractionCheck saved (this is a transient check)

## UC-003 — Background re-verification after med list change

**Actor:** System
**Trigger:** Patient deactivates or adds a medication

**Main flow:**
1. Module 4 fires `MedicationListChanged` event
2. Module 5 event handler queues a re-verification job for each ACTIVE medication
3. Job runs Stage 2-4 (Stage 1 skipped — drug already known)
4. If verdict level CHANGED from last check:
   - Save new InteractionCheck record
   - Send push notification: "Update on your medication X — please review"

## UC-004 — Patient views interaction history

**Actor:** Patient
**Trigger:** Patient taps "Interaction History" in Settings

**Main flow:**
1. App calls `GET /api/interactions/history?patientId=X&limit=20`
2. System returns list of past checks
3. App displays list with colored severity badges
4. Patient taps a row → calls `GET /api/interactions/{id}` for details

## UC-005 — Doctor reviews a patient's medication card

**Actor:** Patient (showing) + Doctor (viewing)
**Preconditions:** Patient generated a Medication Card (Module 8 or sub-feature)

**Main flow:**
1. Card includes a "Recent Interaction Checks" section with last 5 Level 2/3 events
2. Doctor visually scans the card
3. Patient explains the highlighted interactions

(This module supplies the data; the card UI is handled elsewhere.)

---

# PART 7 — DATA REQUIREMENTS

## 7.1 Entity owned: InteractionCheck

```
InteractionCheckId        Guid    PK
PatientId                 Guid    FK → Patient
NewDrugId                 Guid?   FK → Drug (nullable for free-text drugs)
NewDrugName               string  (snapshot at time of check)
Level                     int     (1, 2, or 3)
LabelArabic               string
ExplanationArabic         string  (full text)
RecommendedActionArabic   string
ConflictingDrugsJson      string  (JSON serialized array)
ConflictingConditionsJson string  (JSON serialized array)
SourcesJson               string  (JSON serialized citations)
ModelVersion              string  ("gemini-2.5-flash-2026-05" etc.)
PineconeIndexVersion      string
ConsentRecordId           Guid    FK → ConsentRecord
CheckedAt                 DateTime UTC
IsBackgroundRecheck       bool    (false for user-triggered, true for system re-checks)
PreviousCheckId           Guid?   FK → InteractionCheck (for re-checks)
```

## 7.2 Entities read (from other modules)

- `Patient` (Module 2) — chronic conditions, allergies, age, gender
- `PatientMedication` (Module 4) — active meds for this patient
- `Drug` (Module 8) — drug master records
- `ConsentRecord` (Module 1) — consent linkage for audit

## 7.3 Static reference data needed

**Critical-pair lookup table** (seeded once, updated by admin):
```
CriticalPairId      int      PK
DrugIdA             Guid     FK → Drug
DrugIdB             Guid     FK → Drug
DefaultLevel        int      (3 always for critical pairs)
ReasonArabic        string
Source              string   ("DrugBank ID X" etc.)
```

Examples seeded:
- Warfarin + Aspirin → Level 3, bleeding risk
- Warfarin + Ibuprofen → Level 3, bleeding risk
- ACE inhibitors + Potassium supplements → Level 3, hyperkalemia
- MAOIs + SSRIs → Level 3, serotonin syndrome
- Sildenafil + Nitrates → Level 3, hypotension

**Pregnancy categories** (per FDA):
- A, B, C, D, X classifications per drug
- Stored as metadata on Drug entity

**Allergy cross-reactivity table**:
- Penicillin → Cephalosporins (cross-reactive)
- Sulfa drugs → various
- etc.

---

# PART 8 — API CONTRACTS

## 8.1 Check interaction

```http
POST /api/interactions/check
Authorization: Bearer <patient_jwt>
Content-Type: application/json

Request body:
{
  "patientId": "guid",
  "newDrugId": "guid",        // EITHER this
  "newDrugName": "Ibuprofen",  // OR this (not both)
  "saveResult": true            // false for preview/transient checks
}

Response 200:
{
  "interactionCheckId": "guid",
  "level": 3,
  "labelArabic": "خطر",
  "labelEnglish": "Danger",
  "color": "#D32F2F",
  "icon": "warning_red",
  "explanationArabic": "هذا الدواء يتفاعل مع الوارفارين الذي تتناوله ويزيد من خطر النزيف الحاد.",
  "recommendedActionArabic": "لا تجمع بين هذه الأدوية. توقف فورًا واستشر طبيبك قبل تناول هذا الدواء.",
  "conflictingDrugs": [
    {
      "drugId": "guid",
      "name": "Warfarin",
      "scientificName": "Warfarin Sodium",
      "interactionTypeArabic": "زيادة خطر النزيف",
      "severity": "high"
    }
  ],
  "conflictingConditions": [],
  "conflictingAllergies": [],
  "sources": [
    { "type": "DrugBank", "reference": "DB00682", "citation": "https://..." },
    { "type": "CriticalPair", "ruleId": "CP-0142" }
  ],
  "disclaimerArabic": "استشر طبيبك أو الصيدلي",
  "modelVersion": "gemini-2.5-flash-2026-05",
  "pineconeIndexVersion": "safedose-drugs-v2",
  "checkedAt": "2026-06-06T15:23:11Z"
}

Response 400 — missing required fields
Response 403 — patientId not owned by caller
Response 422 — drug not found in SQL or Pinecone
Response 504 — pipeline timeout (returns precautionary Level 2)
```

## 8.2 Check standalone (two drugs, no patient)

```http
POST /api/interactions/check-standalone
Authorization: Bearer <patient_jwt>

{
  "drugIdA": "guid",
  "drugIdB": "guid"
}

Response 200: (same shape as check, but no conflictingConditions/Allergies)
```

## 8.3 Get history

```http
GET /api/interactions/history?patientId=guid&limit=20&offset=0
Authorization: Bearer <patient_jwt>

Response 200:
{
  "total": 47,
  "items": [
    {
      "interactionCheckId": "...",
      "drugName": "Ibuprofen",
      "level": 3,
      "labelArabic": "خطر",
      "checkedAt": "..."
    }
  ]
}
```

## 8.4 Get check details

```http
GET /api/interactions/{id}
Authorization: Bearer <patient_jwt>

Response 200: (full shape from 8.1)
```

## 8.5 Internal: Patient profile snapshot (for Langflow)

```http
GET /api/internal/patients/{id}/profile-snapshot
Authorization: X-Service-Token <service_token>

Response 200:
{
  "patientId": "guid",
  "age": 62,
  "gender": "male",
  "weightKg": null,
  "isPregnant": false,
  "renalImpairment": false,
  "hepaticImpairment": false,
  "chronicConditions": [
    { "code": "diabetes_type2", "labelArabic": "السكري النوع الثاني" },
    { "code": "hypertension", "labelArabic": "ارتفاع ضغط الدم" }
  ],
  "allergies": [
    { "substance": "penicillin", "severity": "severe" }
  ],
  "currentMedications": [
    {
      "name": "Warfarin",
      "scientificName": "Warfarin Sodium",
      "dose": "5 mg",
      "frequency": "1x daily"
    },
    {
      "name": "Aspocid",
      "scientificName": "Acetylsalicylic Acid",
      "dose": "75 mg",
      "frequency": "1x daily"
    }
  ]
}
```

---

# PART 9 — AI PIPELINE SPECIFICATION

## 9.1 Architecture overview

```
[Input: drugId + patientId]
        ↓
┌─────────────────────────────┐
│  STAGE 1 — Retrieval Agent  │ (Langflow component using Pinecone)
└─────────────────────────────┘
        ↓ (10 candidate drugs)
┌──────────────────────────────────┐
│  STAGE 2 — Patient Profile Agent │ (Custom Component calling .NET API)
└──────────────────────────────────┘
        ↓ (patient context JSON)
┌──────────────────────────────┐
│  STAGE 3 — Comparison Agent  │ (Gemini/Fireworks LLM call)
└──────────────────────────────┘
        ↓ (draft verdict)
┌──────────────────────────────┐
│  STAGE 4 — Validation Agent  │ (Gemini/Fireworks LLM call)
└──────────────────────────────┘
        ↓
[Output: final JSON verdict]
```

## 9.2 Stage 1 — Retrieval Agent

**Job:** Find the new drug (and its variants) in Pinecone.

**Input:**
- `drug_name` (string)

**Logic:**
1. Embed the drug name with bge-m3
2. Query Pinecone top_k=10 with filter on `is_active=true`
3. If top score < 0.55, broaden search (remove filters, try drug class)
4. Return enriched candidate list

**Output:**
```json
{
  "query": "Ibuprofen",
  "candidates": [
    {
      "score": 0.92,
      "drugId": "guid",
      "name": "Brufen 400 mg",
      "scientificName": "Ibuprofen",
      "drugClass": "NSAID",
      "indications": ["pain", "inflammation"]
    },
    ...
  ]
}
```

## 9.3 Stage 2 — Patient Profile Agent

**Job:** Fetch the patient's complete medical context from SQL.

**Input:**
- `patient_id` (string)

**Implementation:** Custom Component in Langflow that calls:
```
GET /api/internal/patients/{id}/profile-snapshot
```

**Output:** the response from 8.5 above.

## 9.4 Stage 3 — Comparison Agent (LLM)

**Job:** Given the new drug + patient context, produce a draft verdict.

**Model:** gemini-2.5-flash OR Qwen3 (via Fireworks)
**Temperature:** 0.1

**System prompt:**

```
You are the drug interaction analysis agent for SafeDose, a medication safety app for Arabic-speaking chronic patients in Egypt.

You receive:
- "new_drug" — the drug the patient wants to add (with scientific name and class)
- "current_medications" — list of drugs the patient is currently taking
- "chronic_conditions" — patient's existing conditions
- "allergies" — patient's known allergies
- "patient_demographics" — age, gender, pregnancy status, organ impairments

YOUR JOB: Identify potential interactions and produce a DRAFT verdict.

═════════════════════════════════════
ANALYZE in this exact order:
═════════════════════════════════════

1. ALLERGY CHECK
   For each allergy, check if new_drug or its known cross-reactives match.
   If match → mandatory Level 3 with allergy explanation.

2. CRITICAL-PAIR CHECK
   For each current med, check known dangerous combinations using your medical knowledge:
   - Warfarin + NSAIDs → bleeding risk
   - ACE inhibitors + Potassium → hyperkalemia
   - MAOIs + SSRIs → serotonin syndrome
   - Sildenafil + Nitrates → severe hypotension
   - And others you know from training.
   If any critical pair → Level 3.

3. CONDITION-DRUG CHECK
   For each chronic condition, check if new_drug is contraindicated:
   - Diabetes + Corticosteroids → caution (blood sugar)
   - Hypertension + NSAIDs → caution (BP elevation)
   - Asthma + Beta blockers → Level 3 (bronchospasm)
   - Renal impairment + many NSAIDs → caution

4. DRUG-DRUG INTERACTION CHECK
   For each pair (new_drug, current_med), use your medical knowledge to identify interactions.
   Classify each as: severe (Level 3), moderate (Level 2), or minor (Level 1 — log but don't escalate).

5. DEMOGRAPHIC CHECK
   - Age > 65 + benzodiazepines → caution
   - Pregnancy + Category D/X drugs → Level 3
   - Pediatric + adult doses → Level 2

═════════════════════════════════════
OUTPUT FORMAT (JSON only):
═════════════════════════════════════

{
  "draft_level": 1 | 2 | 3,
  "reasoning_steps": [
    {"check": "allergy", "result": "no_match", "details": "..."},
    {"check": "critical_pair", "result": "match", "details": "Warfarin + Ibuprofen — bleeding risk", "severity": "severe"},
    ...
  ],
  "conflicting_drugs": [
    {"name": "Warfarin", "interaction_type": "bleeding_risk", "severity": "severe"}
  ],
  "conflicting_conditions": [],
  "conflicting_allergies": [],
  "draft_explanation_arabic": "...",
  "draft_recommended_action_arabic": "...",
  "confidence": "high" | "medium" | "low"
}

Output JSON only. No markdown, no commentary.
```

## 9.5 Stage 4 — Validation Agent (LLM)

**Job:** Apply safety rules, format final output, add citations.

**Model:** Same as Stage 3
**Temperature:** 0.1

**System prompt:**

```
You are the safety validation agent for SafeDose. You receive a DRAFT verdict from the Comparison Agent and your job is to:

1. Verify safety rules are followed
2. Format the final output for the patient
3. Add source citations

═════════════════════════════════════
SAFETY RULES (HARD GUARDS):
═════════════════════════════════════

- NEVER recommend a dose change
- NEVER recommend stopping a medication
- NEVER diagnose a condition
- NEVER substitute a drug name
- If you detect any of the above in the draft, REMOVE that content
- ALWAYS include the disclaimer "استشر طبيبك أو الصيدلي"

═════════════════════════════════════
ARABIC PHRASING RULES:
═════════════════════════════════════

Level 1 (Safe, آمن, green):
- Label: "آمن"
- Action template: "يمكنك تناولهما معاً بأمان"
- Color: "#4CAF50"

Level 2 (Caution, احذر, amber):
- Label: "احذر"
- Action template: "راقب الأعراض، وإذا لاحظت أي تغير استشر طبيبك أو الصيدلي"
- Color: "#FFA000"

Level 3 (Danger, خطر, red):
- Label: "خطر"
- Action template: "لا تجمع بين هذه الأدوية واستشر طبيبك فوراً"
- Color: "#D32F2F"

═════════════════════════════════════
SOURCES:
═════════════════════════════════════

For each finding, add a source. Use these source types:
- "DrugBank" with DB ID
- "FDA" with publication ID
- "CriticalPair" with rule ID
- "Knowledge" for general medical knowledge

═════════════════════════════════════
OUTPUT FORMAT (JSON only):
═════════════════════════════════════

Full verification JSON as specified in API 8.1 response.

Output JSON only. No markdown, no commentary.
```

---

# PART 10 — EDGE CASES & ERROR HANDLING

| Case | Handling |
|------|----------|
| Patient has zero existing meds | Still check against chronic conditions + allergies. Don't skip the pipeline. |
| Drug not in Pinecone | Stage 1 returns empty. Skip Stage 3-4 LLM. Return Level 2 with message "لا يمكننا التحقق تلقائيًا، يرجى استشارة طبيبك." |
| Patient profile is empty (no conditions, no meds, no allergies) | Run pipeline but mention "بيانات المريض غير مكتملة" in the verdict |
| Two drugs with the SAME scientific name (duplicate) | Don't flag as interaction — flag as duplication warning instead |
| Drug interacts with itself (duplicate same drug) | Return Level 2 "هذا الدواء موجود بالفعل في قائمتك" |
| Allergy match found | Hard rule — Level 3 immediately, skip LLM |
| LLM hallucinates a drug | Validation Agent checks that all mentioned drugs are in the candidate list. If hallucinated, downgrade verdict to Level 2 with warning |
| Network failure to Pinecone | Retry 3x, then fall back to critical-pair table only |
| Network failure to LLM | Fall back to critical-pair table; if pair matches → Level 3; otherwise Level 2 |
| Patient adds drug A, then drug B that interacts with A — both checked separately | Both InteractionChecks saved; when B is checked, A appears in conflicting drugs |
| Same drug + patient checked twice within 1 hour | Return cached result, don't re-run pipeline |
| Patient is pregnant + drug is FDA Category C | Return Level 2 with pregnancy-specific message |
| Patient is < 18 (pediatric) | Add pediatric warning to verdict if relevant |
| Drug is herbal supplement | Run check anyway — herbals DO interact (St. John's Wort + many drugs) |

---

# PART 11 — DEPENDENCIES

## 11.1 Module dependencies

| Depends On | What we need from them |
|------------|------------------------|
| Module 1 (Auth) | JWT validation, current account identity |
| Module 2 (Profile) | Patient.ChronicConditions, Patient.Allergies, age, gender |
| Module 4 (Medications) | List of active PatientMedication records |
| Module 8 (Drug Sync) | Up-to-date Drug table + Pinecone index |

## 11.2 External dependencies

| Service | Purpose | Fallback |
|---------|---------|----------|
| Pinecone | Drug retrieval | Critical-pair lookup table |
| Hugging Face (bge-m3) | Embeddings | None (cached embeddings for top 1000 drugs) |
| Gemini / Fireworks LLM | Comparison + Validation | Critical-pair lookup |
| Langflow runtime | Agent orchestration | Direct API calls from .NET (long-term plan) |

## 11.3 Modules that DEPEND on us

| Depends on us | What they call |
|---------------|----------------|
| Module 3 (Prescription Parser) | POST /api/interactions/check after each med confirmation |
| Module 4 (Medication Management) | POST /api/interactions/check-conflicts before saving manual med |
| Module 8 (Medication Card) | GET /api/interactions/history for the card |

---

# PART 12 — TARGETS & METRICS

## 12.1 Functional targets (must hit for demo)

| Target | Acceptance |
|--------|------------|
| Correct Level 3 for Warfarin + Ibuprofen | 100% (hard rule) |
| Correct Level 3 for ACE inhibitors + Potassium | 100% (hard rule) |
| Correct Level 3 for any patient-allergy match | 100% (hard rule) |
| Correct Level 1 for Vitamin D + healthy patient | 100% |
| End-to-end time | < 8 seconds at 95th percentile |
| No silent substitutions | 100% — every check shows the drug name as input |

## 12.2 Quality targets (post-demo, polish)

| Target | Acceptance |
|--------|------------|
| Test set of 50 known interactions | ≥ 90% correctly classified |
| Test set of 20 known safe combinations | ≥ 95% correctly classified as Level 1 |
| Patient satisfaction (out of 5) | ≥ 4.0 in pilot test |
| False positive rate (Level 2/3 when actually safe) | ≤ 15% (conservative bias is OK) |
| False negative rate (Level 1 when actually dangerous) | 0% (this is unacceptable) |

## 12.3 Demo-day metrics to show supervisor

1. Add "Ibuprofen" to a patient on "Warfarin" → Level 3, < 8 seconds, with source
2. Add "Vitamin D" to a healthy patient → Level 1, < 4 seconds
3. Add "Penicillin" to a patient with penicillin allergy → Level 3 (hard rule), < 2 seconds (no LLM call needed)
4. Standalone check: "Sildenafil + Nitrates" → Level 3, < 4 seconds

---

# PART 13 — TESTING STRATEGY

## 13.1 Unit tests (xUnit / NUnit)

Test the core logic without external services:

- `CheckInteractionUseCase` with mocked Pinecone + LLM clients
- Critical-pair lookup table queries
- Severity calculator (input rule results → final level)
- Allergy cross-reactivity matcher
- Arabic message template renderer

**Target:** ≥ 70% code coverage in Application + Domain layers.

## 13.2 Integration tests

Test with real SQL + real Langflow (against staging):

- Full pipeline with 5 realistic patients
- Concurrent requests (10 patients simultaneously)
- Background re-verification trigger
- Service token auth for internal endpoint

## 13.3 End-to-end test scenarios (the demo cases)

| Scenario | Expected Output |
|----------|-----------------|
| Patient profile: hypertension, no allergies, no meds. Add: Ibuprofen. | Level 2 (caution — BP effect) |
| Patient profile: warfarin user. Add: Ibuprofen. | Level 3 (bleeding risk) |
| Patient profile: penicillin allergy. Add: Augmentin. | Level 3 (hard rule, allergy cross-reactivity) |
| Patient profile: diabetic, no meds. Add: Crestor. | Level 1 |
| Patient profile: pregnant. Add: Isotretinoin. | Level 3 (Category X) |
| Patient profile: healthy. Add: Vitamin D. | Level 1 |
| Standalone: Warfarin + Aspirin. | Level 3 (Critical Pair table) |
| Standalone: Paracetamol + Vitamin C. | Level 1 |
| Add unknown drug "Efemyo Eye Drop". | Level 2 (cannot verify) |
| Patient on Warfarin reads card. | History shows last Level 3 check |

## 13.4 Manual testing on real prescriptions

Use 5 real-world prescriptions from clinics (anonymized) and verify outputs make medical sense.

---

# PART 14 — RISKS & MITIGATIONS

| Risk ID | Risk | Likelihood | Impact | Mitigation |
|---------|------|------------|--------|------------|
| R-01 | LLM hallucinates an interaction | Medium | High (patient confused) | Validation Agent cross-checks against candidate list |
| R-02 | LLM misses a critical interaction (false negative) | Medium | CRITICAL (patient harm) | Critical-pair lookup table runs first, BEFORE LLM |
| R-03 | Pinecone returns wrong drug (form mismatch) | Medium | High | Triangle of Trust in Validator + always show patient |
| R-04 | Patient ignores Level 3 warning | High | High (limit our liability) | Require explicit "I will consult my doctor" acknowledgment |
| R-05 | LLM API quota exhausted during demo | Medium | High (demo fails) | Fireworks as backup; critical-pair lookup as offline fallback |
| R-06 | Patient adds wrong drug due to OCR error | Medium | Medium | Module 3 confirmation step; patient sees source name |
| R-07 | Service downtime causes background re-check failures | Low | Low | Idempotent job queue with retries |
| R-08 | Arabic phrasing offensive or unclear | Low | Medium | Native speaker review (Mina + family) |
| R-09 | Egyptian DB missing important drugs | High | Medium | Tier-3 unverified flow + admin queue |
| R-10 | LLM exposes private patient data in response | Low | Critical (compliance) | Output sanitization in Validation Agent |

---

# PART 15 — ACCEPTANCE CRITERIA (Module 5 is DONE when)

Mark each checkbox as you finish:

## Backend
- [ ] All 4 Langflow stages built and wired
- [ ] Patient Profile Agent (Custom Component) calls .NET API and parses JSON correctly
- [ ] Critical-pair lookup table seeded with ≥ 30 known dangerous combinations
- [ ] All 5 API endpoints documented in Swagger with examples
- [ ] Internal endpoint uses service token (not patient JWT)
- [ ] InteractionCheck records save with all required fields
- [ ] Cache layer working (1-hour TTL on identical requests)
- [ ] Retry logic implemented for Pinecone (3x exponential backoff)
- [ ] Fallback to critical-pair table on LLM failure works
- [ ] All hard safety rules pass unit tests (allergy → L3, critical pair → L3)
- [ ] Audit log entries created per check

## AI Quality
- [ ] All 10 demo scenarios pass with expected verdicts
- [ ] No silent substitutions detected in 20 random tests
- [ ] No dose recommendations in any output (regex check)
- [ ] No diagnosis attempts in any output
- [ ] Arabic phrasing reviewed by native speaker
- [ ] Disclaimer present in 100% of outputs

## Frontend (Doaa)
- [ ] Add drug screen with autocomplete
- [ ] Result screen with full-screen colored card per level
- [ ] Sources expandable section
- [ ] Disclaimer footer always visible
- [ ] Level 3 requires "I acknowledge" before saving
- [ ] History list with colored badges
- [ ] History detail view
- [ ] Loading states + error states
- [ ] RTL Arabic renders correctly

## Performance
- [ ] 95th percentile response < 8 seconds (measured over 100 requests)
- [ ] Cached request < 200ms
- [ ] Concurrent 100 users: response time degrades < 50%

## Documentation
- [ ] This requirements doc kept up to date with implementation
- [ ] README in module folder
- [ ] Postman collection committed
- [ ] Architecture diagram in `/docs/architecture.md` updated

---

# PART 16 — TIMELINE & MILESTONES

## Week 1 (current)
- [x] Stage 1 (Retrieval Agent) built in Langflow
- [x] Stage 3 (Comparison Agent) partial
- [ ] Critical-pair lookup table designed (this week)
- [ ] InteractionCheck entity + repository (Mina)

## Week 2
- [ ] Stage 2 (Patient Profile Agent) Custom Component built
- [ ] Internal API endpoint (`/api/internal/patients/{id}/profile-snapshot`)
- [ ] Stage 4 (Validation Agent) built
- [ ] Full pipeline end-to-end test with 5 demo scenarios

## Week 3
- [ ] Public API endpoints implemented (.NET controllers)
- [ ] Cache layer
- [ ] Retry + fallback logic
- [ ] All 10 demo scenarios pass

## Week 4
- [ ] Frontend integration (Doaa)
- [ ] Background re-verification job
- [ ] Integration tests
- [ ] Performance optimization

## Week 5
- [ ] Polish, Arabic review, demo prep
- [ ] Acceptance criteria sign-off
- [ ] Supervisor demo

---

# APPENDIX A — Critical-pair seed list

Mina, seed these 30+ pairs into the `CriticalPair` table on day 1. These are non-negotiable hard rules:

```
PAIR                                  REASON                                LEVEL
Warfarin + Aspirin                    Bleeding risk                         3
Warfarin + Ibuprofen                  Bleeding risk                         3
Warfarin + Clopidogrel                Bleeding risk                         3
Warfarin + Diclofenac                 Bleeding risk                         3
ACE inhibitor + Spironolactone        Hyperkalemia                          3
ACE inhibitor + Potassium supplement  Hyperkalemia                          3
ARB + Spironolactone                  Hyperkalemia                          3
MAOI + SSRI                           Serotonin syndrome                    3
MAOI + Tramadol                       Serotonin syndrome                    3
SSRI + Tramadol                       Serotonin syndrome                    3
SSRI + Triptans                       Serotonin syndrome                    3
Sildenafil + Nitrates                 Severe hypotension                    3
Tadalafil + Nitrates                  Severe hypotension                    3
Statins + Erythromycin                Rhabdomyolysis risk                   3
Statins + Clarithromycin              Rhabdomyolysis risk                   3
Statins + Itraconazole                Rhabdomyolysis risk                   3
Digoxin + Verapamil                   Digoxin toxicity                      3
Digoxin + Amiodarone                  Digoxin toxicity                      3
Beta blocker + Verapamil IV           Cardiac arrest risk                   3
Lithium + Thiazide diuretics          Lithium toxicity                      3
Lithium + NSAIDs                      Lithium toxicity                      3
Methotrexate + NSAIDs                 Methotrexate toxicity                 3
Methotrexate + Trimethoprim           Bone marrow suppression               3
Theophylline + Ciprofloxacin          Theophylline toxicity                 3
Warfarin + Fluconazole                Bleeding risk                         3
Tramadol + Codeine                    Respiratory depression                3
Benzodiazepines + Opioids             Respiratory depression                3
Insulin + Beta blocker (non-cardio)   Hypoglycemia unawareness              2
Metformin + Iodine contrast           Lactic acidosis risk                  2
Furosemide + Aminoglycosides          Ototoxicity                           2
```

Add more as you learn from real prescriptions.

---

# APPENDIX B — Arabic phrasing templates

Use these phrases consistently across all outputs:

**Disclaimers:**
- Standard: "استشر طبيبك أو الصيدلي"
- Emergency: "توقف فورًا واستشر طبيبك"
- Unverified drug: "هذا الدواء غير موثق في قاعدة بياناتنا"

**Action templates:**
- Level 1: "يمكنك تناولهما معاً بأمان"
- Level 2 (drug-drug): "راقب الأعراض، وإذا لاحظت أي تغير استشر طبيبك"
- Level 2 (condition): "احذر — هذا الدواء قد يؤثر على حالة [condition]"
- Level 3 (drug-drug): "لا تجمع بين هذه الأدوية واستشر طبيبك فوراً"
- Level 3 (allergy): "تنبيه: هذا الدواء يحتوي على مادة لديك حساسية منها"
- Level 3 (pregnancy): "هذا الدواء غير آمن أثناء الحمل"

**Symptoms to watch (for Level 2 messages):**
- General: "أعراض غير معتادة"
- Bleeding: "نزيف أو كدمات غير عادية"
- Cardiac: "خفقان قوي أو دوخة"
- Allergic: "طفح جلدي، حكة، أو صعوبة في التنفس"

---

# APPENDIX C — Decision tree (quick reference)

```
Input: new drug + patient context
    │
    ▼
[Allergy match?] ──── Yes ──► Level 3 (allergy)
    │
    No
    ▼
[Drug in Pinecone?] ──── No ──► Level 2 (cannot verify)
    │
    Yes
    ▼
[Critical-pair match?] ──── Yes ──► Level 3 (critical pair)
    │
    No
    ▼
[Pregnancy + Category D/X?] ──── Yes ──► Level 3 (pregnancy)
    │
    No
    ▼
[Run Comparison Agent (LLM)]
    │
    ▼
[Run Validation Agent (LLM)]
    │
    ▼
[Final level: 1, 2, or 3]
    │
    ▼
Save InteractionCheck
Return verdict
```

---

# END OF MODULE 5 REQUIREMENTS

This document is the contract. As you build, EVERY decision must trace back to an FR/NFR/UC ID. If you find a gap, update this doc FIRST, then code it.

— Mina
