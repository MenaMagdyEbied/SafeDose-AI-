# SafeDose AI — Frontend (Angular PWA)
## Structure (when initialized)

```
safedose-app/src/app/
├── core/         Singletons (auth service, API interceptor, guards)
├── shared/       Shared components (header, footer, loading)
├── features/
│   ├── auth/              Phone OTP + profile setup
│   ├── drug-interaction/  Hero feature — Level 1/2/3 checker
│   ├── medications/       Add prescription, medication list
│   ├── reminders/         Today's reminders, response
│   ├── chatbot/           Floating chatbot widget
│   ├── medication-card/   Printable card with QR
│   ├── clinic-visits/     Visit timeline
│   ├── settings/          User settings
│   └── admin/             Admin dashboard
└── app.component.ts
```


