# Frontend Field Guide — Doaa's Reference

> What every form and screen needs, mapped to backend DTOs.
> Use this when building Angular reactive forms / validators.
> Updated: 2026-06-08

---

## Conventions

- **Required** fields throw 400 from backend if missing
- **Optional** fields can be `null` / omitted
- **Enum** fields: send numeric codes, render Arabic labels — backend returns both
- **Dates** use ISO format `YYYY-MM-DD` (DateOnly) or full ISO 8601 (DateTime)
- **Errors** always come back as `{ code, messageArabic, messageEnglish?, details? }`

---

## MODULE 1 — Auth (Andrew)

### Form: Register
**Endpoint:** `POST /api/Auth/register`

| Field | Type | Required | Notes |
|---|---|---|---|
| name | string | ✅ | Display name, 2-150 chars |
| email | string | ✅ | Must be valid email + unique |
| password | string | ✅ | Min length depends on Identity config |
| phoneNumber | string | optional | E.164 format `+20...` |

### Form: Login
**Endpoint:** `POST /api/Auth/login`

| Field | Type | Required |
|---|---|---|
| email | string | ✅ |
| password | string | ✅ |

**Response:** `{ token, expiresOn, isAuthenticated, message }` — store `token` and send as `Authorization: Bearer <token>` on every subsequent call.

---

## MODULE 2 — Patient Profile (Fady)

### Form: Create Patient
**Endpoint:** `POST /api/patients`

| Field | Type | Required | Rules |
|---|---|---|---|
| fullName | string | ✅ | 2-150 chars, trimmed |
| dateOfBirth | DateOnly `YYYY-MM-DD` | optional | Between 1900-01-01 and today |
| gender | byte (1/2/3) | optional | 1=Male, 2=Female, 3=Other |
| bloodType | string | optional | One of: `A+`, `A-`, `B+`, `B-`, `AB+`, `AB-`, `O+`, `O-` |
| chronicConditions | string[] | optional | Send as array of tags, backend stores as CSV |
| allergies | string[] | optional | Same — array → CSV |

### Form: Update Patient
**Endpoint:** `PUT /api/patients/{id}`

Same fields as Create, but ALL optional (partial update). Send only changed fields.

### List screen: My Patients
**Endpoint:** `GET /api/patients/my?includeInactive=false`

Returns array of `PatientResponseDto`. Each row should show: fullName, age (computed from DOB), gender icon, blood-type chip, IsActive badge.

### Detail screen
**Endpoint:** `GET /api/patients/{id}`

Returns full patient — show all fields, with chronicConditions and allergies as removable tag chips.

### Deactivate (soft delete)
**Endpoint:** `DELETE /api/patients/{id}` → 204

Show confirmation modal: "هل أنت متأكد من تعطيل هذا الملف؟ يمكنك استعادته لاحقاً." Backend doesn't actually delete — just hides from default list.

---

## MODULE 4 — Medication Management (Ahmed)

### Form: Add Medication Manually
**Endpoint:** `POST /api/medications`

| Field | Type | Required | Rules / UI hint |
|---|---|---|---|
| patientId | int | ✅ | Selected from "Which patient?" dropdown |
| drugId | int | ✅ | From autocomplete — see Drug Search below |
| dose | string | optional | Free text — "500 mg", "1/2 tablet", "5 drops" |
| frequency | int | optional | Times per day, 1-12 — render as +/- stepper |
| startDate | DateOnly | optional | Defaults to today if omitted |
| endDate | DateOnly | optional | Must be ≥ startDate; null = ongoing |
| mealTiming | byte (1-4) | optional | See enum below |
| prescriptionId | int | optional | Only set when coming from Module 3 OCR flow |

### Bulk Add from Prescription
**Endpoint:** `POST /api/medications/from-prescription`

Called automatically from Module 3's "Confirm OCR results" screen. UI sends:
```json
{
  "patientId": 7,
  "prescriptionId": 42,
  "medications": [
    { "drugId": 1234, "dose": "500 mg", "frequency": 2, "mealTiming": 3, "startDate": "2026-06-08", "endDate": "2026-06-15" }
  ]
}
```

Max 20 items. All drugIds must exist or the entire batch fails (transactional).

### List screen: Active Meds for Patient
**Endpoint:** `GET /api/medications/patient/{patientId}`

Returns array of `MedicationResponseDto`. Each row shows:
- Drug name + dose
- Frequency (e.g. "3 مرات يوميًا")
- MealTimingArabic ("قبل الأكل" etc.)
- StatusArabic chip ("نشط")
- Action buttons: Edit, Pause, Stop

### History screen
**Endpoint:** `GET /api/medications/patient/{patientId}/history`

Returns `{ active: [...], paused: [...], stopped: [...] }`. Render as 3 collapsible sections.

### Edit form
**Endpoint:** `PUT /api/medications/{id}`

Editable: dose, frequency, startDate, endDate, mealTiming. **NOT** editable: drugId, patientId — those are immutable after creation. UI should grey them out.

### Status actions
- `POST /api/medications/{id}/pause` — 204
- `POST /api/medications/{id}/resume` — 204
- `POST /api/medications/{id}/stop` — 204 (irreversible — confirm modal!)

**State machine rules** (enforced by backend; UI should respect):
- Active ↔ Paused: free
- Active → Stopped: allowed
- Paused → Stopped: allowed
- Stopped → anything: **blocked** (UI should hide pause/resume buttons for stopped meds)

### Drug Search (autocomplete)
**Endpoint:** `GET /api/drugs/search?q={query}&limit=10`

Use for the "Drug" field in Add Medication form. Returns top matches as user types:
```json
[
  { "drugId": 1234, "drugName": "Aspirin", "scientificName": null, "drugClass": null, "commonDose": "100 mg" }
]
```

Show `drugName` as primary, `commonDose` as subtext. Send the selected `drugId` to the backend.

---

## MODULE 5 — Drug Interaction Checker (Mina)

### Form: Multi-drug check
**Endpoint:** `POST /api/interactions/check`

| Field | Type | Required | Rules |
|---|---|---|---|
| drugIds | int[] | ✅ | 1-6 drugs (UI cap) |
| patientId | int | optional | Adds patient context for personalized check |
| triggerType | byte | optional | 1=Manual, 2=Prescription, 3=Barcode, 4=Voice |

### Response — render the Page 2 result card
```json
{
  "interactionCheckId": 123,
  "level": 3,                                  // 1=safe, 2=caution, 3=danger
  "labelArabic": "خطر",                         // big title chip
  "color": "#D32F2F",                          // hex for card background
  "titleArabic": "تفاعل دوائي ذو خطورة...",
  "explanationArabic": "...",
  "recommendedActionArabic": "...",
  "analyzedDrugs": [
    { "drugId": 1, "arabicName": "الأسبرين", "englishName": "Aspirin", "dosageNote": "...", "role": "primary" }
  ],
  "conflictingPairs": [
    { "drugA": "Aspirin", "drugB": "Warfarin", "reasonArabic": "...", "severity": "high" }
  ],
  "sources": ["DrugBank DB00682"],
  "safetyDisclaimerArabic": "استشر طبيبك أو الصيدلي",
  "checkedAt": "2026-06-08T15:00:00Z"
}
```

### History
**Endpoint:** `GET /api/interactions/history?patientId=7&limit=20&offset=0`

Returns `{ total, limit, offset, items: [{ interactionCheckId, level, labelArabic, color, drugNames, checkedAt }] }`. Use `total` for "Load more" button.

### Acknowledge a Level 3 result
**Endpoint:** `POST /api/interactions/{id}/acknowledge` — 204

UI flow: when Level 3 result shown, the "Save anyway" button is disabled until user taps a checkbox "أتعهد باستشارة طبيبي". On tap → call this endpoint, then enable the save.

---

## ENUMS — Single source of truth

### Gender (Patient.gender)
```ts
const GENDER = { 1: 'ذكر', 2: 'أنثى', 3: 'آخر' };
```

### Status (PatientMedication.status)
```ts
const MED_STATUS = { 1: 'نشط', 2: 'متوقف مؤقتاً', 3: 'متوقف' };
const MED_STATUS_COLOR = { 1: '#4CAF50', 2: '#FFA000', 3: '#9E9E9E' };
```

### MealTiming (PatientMedication.mealTiming)
```ts
const MEAL_TIMING = {
  null: 'في أي وقت',
  1: 'قبل الأكل',
  2: 'مع الأكل',
  3: 'بعد الأكل',
  4: 'قبل النوم'
};
```

### InteractionLevel
```ts
const LEVEL = { 1: 'آمن', 2: 'احذر', 3: 'خطر' };
const LEVEL_COLOR = { 1: '#4CAF50', 2: '#FFA000', 3: '#D32F2F' };
```

### BloodType (Patient.bloodType)
```ts
const BLOOD_TYPES = ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'];
```

### TriggerType (InteractionCheck.triggerType)
```ts
const TRIGGER = { 1: 'يدوي', 2: 'من وصفة', 3: 'باركود', 4: 'صوتي' };
```

---

## Error handling — show this to user

Every error response from any module looks like:
```json
{
  "code": "VALIDATION_FAILED",
  "messageArabic": "البيانات المدخلة غير صحيحة",
  "messageEnglish": "Validation failed",
  "details": "FullName must be 2-150 characters"
}
```

UI rule: always show `messageArabic` to the user as a toast/snackbar. Log `details` to console for debugging.

Common codes:
- `VALIDATION_FAILED` (400)
- `NOT_FOUND` (404)
- `UNAUTHORIZED` (401) → force re-login
- `FORBIDDEN` (403) → show "غير مصرح لك"
- `TOO_MANY_DRUGS` (400) → show "الحد الأقصى 6 أدوية"
- `DRUG_NOT_FOUND` (400) → re-search the catalog
- `LANGFLOW_UNAVAILABLE` (504) → show "حاول لاحقاً"

---

— Mina
