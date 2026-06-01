# SafeDose AI — Frontend (Angular PWA)

## Setup

1. Install Node.js 20+ and npm
2. Install Angular CLI: `npm install -g @angular/cli`
3. From the `safedose-app/` folder:
   ```
   cd safedose-app
   npm install
   npm start
   ```
4. App available at http://localhost:4200

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

## Design System

- **Primary color:** `#005EB8` (medical blue)
- **Secondary:** `#00A3AD` (teal)
- **Font:** IBM Plex Sans Arabic
- **Layout:** RTL by default with LTR toggle
- **Tap targets:** minimum 44px (elderly-friendly)

## PWA Features

- Service worker for offline
- Add to home screen
- Push notification support

## Owners

- Lead: Mina (Drug Interaction UI)
- Auth screens: Doaa
- Medication screens: Ahmed
- Reminders / Card / Visits: Andrew
- Chatbot widget / Admin: Fady
