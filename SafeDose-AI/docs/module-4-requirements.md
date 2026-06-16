# Module 4 — Medication Management — Requirements Specification

> **Owner:** Ahmed
> **Status:** Building now (entity already exists)
> **Depends on:** Module 1 (Auth), Module 2 (Patient)
> **Last Updated:** 2026-06-08

---

## 1. Why this module exists

The bridge between the **drug catalog** (22,500 entries) and the **patient's actual medication list**.

Module 3 (Prescription Parser) extracts drugs from photos. But the OCR just gives us names — it doesn't tell us:
- The DOSE this patient actually takes
- HOW OFTEN they take it
- WHEN (with meals? before bed?)
- WHETHER they're currently taking it or stopped

`PatientMedication` answers all of this. It is the LIVE record of "what is this patient taking right now."

Module 4 owns the CRUD on this entity.

## 2. Scope

### IN scope
- Add a single medication manually (typing the drug name)
- Bulk-add medications from a confirmed prescription (called by Module 3)
- Update dose / frequency / start date / end date / meal timing
- Pause a medication (temporary stop)
- Resume a paused medication
- Stop a medication permanently (still in DB for history)
- List active medications for a patient
- View full medication history (active + paused + stopped)
- Get single medication details

### OUT of scope
- Reminder schedule (`PatientMedicationTime`) → Module 6
- Taken/skipped log (`ReminderResponse`) → Module 6
- Prescription image / OCR text → Module 3
- Drug catalog edits → Module 9
- Refill ordering → not in ERD
- Doctor notifications → not in ERD

## 3. The PatientMedication entity (existing — no changes)

```csharp
public class PatientMedication
{
    public int PatientMedicationId { get; set; }
    public int PatientId { get; set; }              // FK → Patient
    public int DrugId { get; set; }                 // FK → Drug (22,500 catalog)
    public int? PrescriptionId { get; set; }        // FK → source Prescription (null if manual)
    public string? Dose { get; set; }               // e.g., "500 mg" or "1/2 tablet"
    public int? Frequency { get; set; }             // times per day
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public byte? MealTiming { get; set; }           // 1=before, 2=with, 3=after, 4=bedtime
    public byte Status { get; set; }                // 1=active, 2=paused, 3=stopped
    public string AccountId { get; set; }           // denormalized for fast ownership check
    // navigation...
}
```

## 4. Functional requirements

### FR-400 series — Add

| ID | Requirement |
|----|-------------|
| FR-401 | The system SHALL allow adding a medication manually with DrugId + dose + frequency |
| FR-402 | The system SHALL allow bulk-adding medications from a confirmed prescription (called by Module 3) |
| FR-403 | Adding a medication SHALL trigger Module 5 interaction check (asynchronously, non-blocking) |
| FR-404 | New medications default to Status=1 (active) and StartDate=today if not specified |
| FR-405 | The system SHALL reject adding a medication if the DrugId doesn't exist in the catalog |

### FR-410 series — Update

| ID | Requirement |
|----|-------------|
| FR-411 | The system SHALL allow updating dose, frequency, start date, end date, meal timing |
| FR-412 | The system SHALL NOT allow changing DrugId or PatientId (immutable after creation) |
| FR-413 | Changing frequency or meal timing SHALL trigger Module 6 reminder regeneration |

### FR-420 series — Lifecycle (status transitions)

| ID | Requirement |
|----|-------------|
| FR-421 | A medication SHALL transition Active(1) ↔ Paused(2) freely |
| FR-422 | A medication SHALL transition any → Stopped(3) — but Stopped cannot transition back |
| FR-423 | Resuming a paused medication SHALL trigger Module 5 interaction check (in case patient added new drugs while paused) |
| FR-424 | Background job SHALL auto-stop medications with EndDate < today (Status → 3) |

### FR-430 series — Read

| ID | Requirement |
|----|-------------|
| FR-431 | The system SHALL list active (Status=1) medications for a patient by default |
| FR-432 | The system SHALL provide full history grouped by status |
| FR-433 | List queries SHALL include the Drug entity (name, route, etc.) via Include |

### FR-440 series — Security

| ID | Requirement |
|----|-------------|
| FR-441 | All endpoints SHALL require valid JWT |
| FR-442 | Ownership: medications SHALL only be accessible to the account that owns the parent Patient |
| FR-443 | Every write SHALL write an entry to AuditLog (Egyptian DPL 151/2020) |

## 5. Non-functional requirements

| ID | Requirement |
|----|-------------|
| NFR-401 | Active medications list SHALL respond in <300ms (indexed on PatientId + Status) |
| NFR-402 | All errors SHALL be in Arabic via ErrorResponse |
| NFR-403 | Bulk-add SHALL handle up to 20 medications in one request |

## 6. API endpoints

### Add a single medication manually
```http
POST /api/medications
{
  "patientId": 7,
  "drugId": 1234,
  "dose": "500 mg",
  "frequency": 2,
  "startDate": "2026-06-08",
  "endDate": "2026-07-08",
  "mealTiming": 3
}
```

### Bulk add from prescription
```http
POST /api/medications/from-prescription
{
  "patientId": 7,
  "prescriptionId": 42,
  "medications": [
    { "drugId": 1234, "dose": "500 mg", "frequency": 2, "mealTiming": 3, "startDate": "2026-06-08", "endDate": "2026-06-15" },
    { "drugId": 5678, "dose": "1 drop", "frequency": 3, "mealTiming": null, "startDate": "2026-06-08", "endDate": null }
  ]
}
```

### List active medications
```http
GET /api/medications/patient/{patientId}
```

### Full history
```http
GET /api/medications/patient/{patientId}/history
```

### Single medication
```http
GET /api/medications/{id}
```

### Update
```http
PUT /api/medications/{id}
{ any editable fields }
```

### Status transitions
```http
POST /api/medications/{id}/pause
POST /api/medications/{id}/resume
POST /api/medications/{id}/stop
```

## 7. Status enum (must match across modules)

| Value | Meaning | Arabic |
|---|---|---|
| 1 | Active | نشط |
| 2 | Paused | متوقف مؤقتاً |
| 3 | Stopped | متوقف |

## 8. MealTiming enum

| Value | Meaning | Arabic |
|---|---|---|
| 1 | Before meals | قبل الأكل |
| 2 | With meals | مع الأكل |
| 3 | After meals | بعد الأكل |
| 4 | Bedtime | قبل النوم |
| null | Anytime | في أي وقت |

## 9. Acceptance criteria

- [ ] All 9 endpoints work end-to-end via Postman
- [ ] Ownership enforced (403 if not your patient)
- [ ] Status transitions enforced (can't unstop)
- [ ] Adding/resuming triggers Module 5 (CheckDrugInteractionUseCase) call
- [ ] Bulk add validates ALL drugs exist before creating any
- [ ] Background job for auto-stop is registered as IHostedService
- [ ] Every write writes to AuditLog
- [ ] All Arabic error messages in ErrorResponse

— Mina
