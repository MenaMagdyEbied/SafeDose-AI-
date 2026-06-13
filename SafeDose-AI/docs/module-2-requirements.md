# Module 2 — Patient Profile — Requirements Specification

> **Owner:** Fady
> **Status:** Not started — entity exists, no use cases/controller yet
> **Depends on:** Module 1 (Auth — Andrew, DONE)
> **Last Updated:** 2026-06-07

---

## 1. Why this module exists

After a user signs up (Module 1 gives them an Account), we need to know WHO they are MEDICALLY. The Patient record holds:
- Full name + demographics (age, gender, blood type)
- **Chronic conditions** (diabetes, hypertension, etc.)
- **Known allergies** (penicillin, NSAIDs, etc.)

This data is the FUEL for Module 5's Drug Interaction Checker. Without it, interaction checks are blind. With it, we can catch allergy matches + condition-drug contraindications.

## 2. Scope — strict (no extras)

### IN scope
- Create a Patient linked to an Account
- View own patient(s) — one Account can have multiple Patients (family feature later)
- Edit Patient profile (name, DOB, gender, blood type, conditions, allergies)
- Soft delete (deactivate) a Patient
- List all Patients for the current account

### OUT of scope (do NOT build)
- Pregnancy field (not in ERD)
- Doctor entity (no Doctor table in current ERD — defer)
- Family-account subscription gate (Module 10 owns this)
- Photo uploads (not in ERD)
- Insurance info (not in ERD)
- Emergency contact (not in ERD)
- Anything else

**RULE:** if a field isn't on the existing `Patient` entity, we DO NOT add it. We match the database, nothing more.

## 3. The existing Patient entity (what we work with)

```csharp
public class Patient
{
    public int PatientId { get; set; }
    public string AccountId { get; set; }                  // FK to Identity Account (string)
    public string FullName { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
    public byte? Gender { get; set; }                       // 1=Male, 2=Female, 3=Other
    public string? BloodType { get; set; }                  // "O+", "A-", etc.
    public string? ChronicConditions { get; set; }          // CSV: "diabetes_type2,hypertension"
    public string? Allergies { get; set; }                  // CSV: "penicillin,sulfa"
    public DateTime CreatedAt { get; set; }

    public Account Account { get; set; } = null!;
    public ICollection<Prescription>? Prescriptions { get; set; }
    public ICollection<PatientMedication>? PatientMedications { get; set; }
    public ICollection<InteractionCheck>? InteractionChecks { get; set; }
    public ICollection<SymptomReport>? SymptomReports { get; set; }
    public ICollection<ClinicDescriptionReminder>? ClinicDescriptionReminders { get; set; }
}
```

**Important:** the entity does NOT currently have `IsActive` or `DeletedAt`. For soft delete we'll need to add them — OR we keep "soft delete" out of scope for V1 and only support read/update/create. Decide with team.

For this requirements doc we **add** `IsActive` (bool, default true) and `DeactivatedAt` (DateTime?, nullable) — minimal entity change, supports soft delete.

## 4. Functional requirements

### FR-200 series — CRUD

| ID | Requirement |
|----|-------------|
| FR-201 | The system SHALL allow the authenticated user to create a Patient linked to their Account |
| FR-202 | The system SHALL allow viewing all Patients belonging to the current Account |
| FR-203 | The system SHALL allow viewing a specific Patient by ID, but only if owned by the current Account |
| FR-204 | The system SHALL allow updating any editable field of an owned Patient |
| FR-205 | The system SHALL allow soft-deleting (deactivating) an owned Patient |
| FR-206 | The system SHALL NEVER hard-delete Patient data (medical record retention) |
| FR-207 | Deactivated Patients SHALL be hidden from default list view |
| FR-208 | The system SHALL prevent listing/editing/viewing Patients NOT owned by the current Account (403 Forbidden) |
| FR-209 | First-time setup: after consent, the user is forced to create at least one Patient before other modules become accessible |

### FR-300 series — Validation

| ID | Requirement |
|----|-------------|
| FR-301 | FullName SHALL be 2-150 characters, non-empty, trimmed |
| FR-302 | DateOfBirth SHALL be between 1900-01-01 and today |
| FR-303 | Gender SHALL be 1, 2, or 3 (or null) |
| FR-304 | BloodType SHALL be one of: "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" (or null) |
| FR-305 | ChronicConditions SHALL be a comma-separated string, max 1000 chars |
| FR-306 | Allergies SHALL be a comma-separated string, max 1000 chars |
| FR-307 | The system SHALL accept structured tags (array of strings) OR free text and JOIN them into CSV before saving |

### FR-400 series — Audit

| ID | Requirement |
|----|-------------|
| FR-401 | Every create/update/delete SHALL write an entry to AuditLog (Egyptian Data Protection Law 151/2020) |
| FR-402 | AuditLog entries SHALL include: AccountId, EntityName="Patient", EntityRowId, ActionType, Timestamp |

## 5. Non-functional requirements

| ID | Requirement |
|----|-------------|
| NFR-201 | All endpoints SHALL require valid JWT (Module 1 auth) |
| NFR-202 | List endpoint SHALL respond in <300ms (indexed on AccountId) |
| NFR-203 | All error messages SHALL be in Arabic via standardized ErrorResponse |
| NFR-204 | The Patient entity SHALL have an index on AccountId for fast listing |

## 6. API endpoints

### 1. Create patient
```http
POST /api/patients
Authorization: Bearer <jwt>

{
  "fullName": "محمد علي",
  "dateOfBirth": "1965-03-15",
  "gender": 1,
  "bloodType": "O+",
  "chronicConditions": ["diabetes_type2", "hypertension"],
  "allergies": ["penicillin"]
}

Response 201:
{
  "patientId": 42,
  "fullName": "...",
  ...
}
```

### 2. List my patients
```http
GET /api/patients/my
Authorization: Bearer <jwt>

Response 200:
[
  { "patientId": 42, "fullName": "...", "dateOfBirth": "...", "isActive": true }
]
```

### 3. Get patient by ID
```http
GET /api/patients/{id}
Authorization: Bearer <jwt>

Response 200 — full patient
Response 403 — not your patient
Response 404 — doesn't exist
```

### 4. Update patient
```http
PUT /api/patients/{id}
Authorization: Bearer <jwt>

{ any editable fields }

Response 200
```

### 5. Deactivate patient
```http
DELETE /api/patients/{id}
Authorization: Bearer <jwt>

Response 204
Logic: sets IsActive = false + DeactivatedAt = now
```

## 7. Data model changes needed

Minimal additions to `Patient.cs`:

```csharp
public bool IsActive { get; set; } = true;
public DateTime? DeactivatedAt { get; set; }
```

EF configuration update:
- HasIndex on AccountId (for fast listing)
- HasQueryFilter for soft delete (hide deactivated from default queries)

## 8. Use cases (5 total)

| Use Case | Job |
|----------|-----|
| CreatePatientUseCase | Validate input, link to Account, save, audit |
| UpdatePatientUseCase | Verify ownership, update editable fields, audit |
| GetMyPatientsUseCase | List patients for current Account |
| GetPatientByIdUseCase | Fetch single patient, verify ownership |
| DeactivatePatientUseCase | Soft delete (IsActive=false), audit |

## 9. Dependencies

| Depends on | What we use |
|------------|-------------|
| Module 1 (Auth) | JWT validation, current AccountId from claims |
| AuditLog | Compliance writes (via IAuditLogService from Module 5) |

## 10. Modules that depend on this

| Module | What they call |
|--------|----------------|
| Module 5 (Interaction) | Reads Patient via IPatientRepository for context |
| Module 7 (Chatbot) | Reads Patient for context |
| Module 3 (Prescription) | Saves prescriptions linked to a Patient |

## 11. Acceptance criteria (DONE when)

- [ ] Patient entity has IsActive + DeactivatedAt fields
- [ ] EF migration applied
- [ ] 5 use cases implemented + unit-testable (no SQL in them)
- [ ] PatientsController with 5 endpoints in Swagger
- [ ] All endpoints require JWT
- [ ] Ownership check on all single-patient operations (403 if not owner)
- [ ] Soft delete works — deactivated patients hidden from /my list
- [ ] Every write triggers an AuditLog entry
- [ ] CSV roundtrip works (UI sends array → we save CSV → we return array)
- [ ] Postman tested end-to-end

## 12. Out of scope — explicit list

Things teams might be tempted to add but we WON'T (no ERD support):
- Patient photo
- Insurance card
- Emergency contact name/phone
- Pregnancy status
- Weight, height, BMI
- Doctor selection (no Doctor entity exists)
- Multi-language patient name
- Caregiver assignment

If anyone wants these later, update the ERD first, then update this requirements doc, THEN build.

---

— Mina
