# API Contracts

The .NET Core API exposes endpoints that the Angular PWA calls. The backend internally calls Langflow agents.

## Authentication

### POST /api/auth/send-otp
```json
Request:  { "phone": "+201XXXXXXXXX" }
Response: { "sent": true, "expiresIn": 300 }
```

### POST /api/auth/verify-otp
```json
Request:  { "phone": "+201XXXXXXXXX", "code": "123456" }
Response: { "token": "JWT...", "isNewUser": true, "patientId": "guid" }
```

## Drug Interaction

### POST /api/interactions/check
```json
Request:  { "patientId": "guid", "drugs": ["Concor", "Glucophage"] }
Response: {
  "level": 2,
  "color": "amber",
  "label_ar": "احذر",
  "explanation_ar": "...",
  "recommended_action_ar": "راقب الأعراض، استشر طبيبك",
  "sources": ["DrugBank", "EDA registry"]
}
```

## Prescriptions

### POST /api/prescriptions (manual)
```json
Request:  { "patientId": "guid", "drugs": [{ "name": "...", "dose": "...", "frequency": "..." }] }
Response: { "prescriptionId": "guid", "medicationIds": [...] }
```

### POST /api/prescriptions/photo (OCR)
```json
Request:  multipart/form-data with image
Response: { "extractedDrugs": [...], "confirmationRequired": true }
```

## Reminders

### GET /api/reminders/today
```json
Response: [
  { "id": "guid", "medicationName": "...", "scheduledAt": "08:00", "status": "pending" }
]
```

### POST /api/reminders/{id}/respond
```json
Request:  { "response": "taken" | "skipped" | "snoozed", "snoozeMinutes": 15 }
Response: { "ok": true }
```

## Chatbot

### POST /api/chat/send
```json
Request:  { "patientId": "guid", "message": "ما هو دواء جلوكوفاج؟" }
Response: {
  "reply_ar": "...",
  "is_symptom_triage": false,
  "severity_level": null,
  "sources": ["DrugBank"]
}
```

For symptom-like messages, `is_symptom_triage` is `true` and `severity_level` is 1, 2, or 3.

## Clinic Visits

### POST /api/visits
```json
Request:  { "patientId": "guid", "date": "...", "doctorName": "...", "notes": "..." }
Response: { "visitId": "guid" }
```

### GET /api/visits
```json
Response: [{ "id": "...", "date": "...", "doctorName": "...", ... }]
```

## Medication Card

### GET /api/patient/{id}/medication-card
```json
Response: {
  "patientName": "...",
  "activeMedications": [...],
  "lastUpdated": "...",
  "qrCode": "data:image/png;base64,..."
}
```

## Admin (separate routes)

### GET /api/admin/overview
### GET /api/admin/users
### PUT /api/admin/pricing-tiers/{id}

(Requires admin JWT.)
