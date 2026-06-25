# Admin Dashboard Module

Powers the Angular SuperAdmin dashboard. Lives inside the main `SafeDose.*` projects
under `Admin/` subfolders so it shares the DbContext, JWT config, and Identity setup.

## What's where

| Layer | Path |
|---|---|
| Controllers | `SafeDose.Api/Controllers/Admin/` |
| Use cases | `SafeDose.Application/UseCases/Admin/{Auth,Dashboard,PricingTiers,Accounts}/` |
| DTOs | `SafeDose.Application/DTOs/Admin/` |
| Repository interfaces | `SafeDose.Application/Interfaces/Admin/` |
| Repository implementations | `SafeDose.Infrastructure/Repositories/Admin/` |
| Cache + background refresh | `SafeDose.Application/Caching/` + `SafeDose.Application/BackgroundJobs/` |
| New entity | `SafeDose.Domain/Entities/PricingTierFeature.cs` |
| Schema script | `backend/database/admin_dashboard_schema.sql` |

## Endpoints

| Method | Route | Roles |
|---|---|---|
| POST | `/api/admin/auth/login` | (anonymous) |
| GET | `/api/admin/dashboard/kpis` | SuperAdmin, Admin |
| GET | `/api/admin/dashboard/revenue?period=monthly\|yearly` | SuperAdmin, Admin |
| GET | `/api/admin/dashboard/users/gender` | SuperAdmin, Admin |
| GET | `/api/admin/dashboard/treatment-cards` | SuperAdmin, Admin |
| GET | `/api/admin/dashboard/team` | SuperAdmin, Admin |
| GET | `/api/admin/dashboard/users/free-vs-paid` | SuperAdmin, Admin |
| GET | `/api/admin/dashboard/activities/recent?limit=10` | SuperAdmin, Admin |
| GET | `/api/admin/pricing-tiers` | SuperAdmin, Admin |
| PUT | `/api/admin/pricing-tiers/{id}` | SuperAdmin, Admin |
| POST | `/api/admin/pricing-tiers/{id}/features` | SuperAdmin, Admin |
| DELETE | `/api/admin/pricing-tiers/{id}/features/{featureId}` | SuperAdmin, Admin |
| GET | `/api/admin/admins?page=1&pageSize=20` | SuperAdmin only |
| POST | `/api/admin/admins` | SuperAdmin only |
| PUT | `/api/admin/admins/{id}` | SuperAdmin only |
| DELETE | `/api/admin/admins/{id}` | SuperAdmin only |
| PATCH | `/api/admin/admins/{id}/status` | SuperAdmin only |

## Setup (one time)

**Step 1 — Apply schema.** Pick ONE of these two paths:

**Path A — Run the SQL directly (fastest):**
```
sqlcmd -S . -d SafeDose -i backend/database/admin_dashboard_schema.sql
```

**Path B — EF migration (recommended for production):**
```
dotnet ef migrations add AddAdminDashboardSupport --project SafeDose.Domain --startup-project SafeDose.Api
dotnet ef database update --project SafeDose.Domain --startup-project SafeDose.Api
```

Don't run both — they do the same thing.

**Step 2 — Build:**
```
dotnet restore
dotnet build
```

**Step 3 — Run.** Existing SuperAdmin (seeded in main backend) works for first login:
- Email: `superadmin@gmail.com`
- Password: `SuperAdmin@123`

## Performance

`DashboardCacheRefreshService` recalculates every dashboard panel every 1 hour and stores
the result in `IMemoryCache`. The controller serves cached payloads first — sub-100ms
even with thousands of admin loads. Cold start (first 15 seconds) falls back to running
the use case directly so no endpoint ever returns 404 because of an empty cache.

## Angular notes

- The Angular client attaches `Authorization: Bearer <token>` on every request after
  `/api/admin/auth/login`. JWT uses the SAME `JWT:Key` as the main backend, so the
  existing JwtBearer middleware validates admin tokens automatically.
- Arabic strings come back in fields suffixed with `Arabic` (`MonthLabelArabic`,
  `TierNameArabic`, `TitleArabic`, `messageArabic`). Bind those directly.
- CORS is already configured by the main backend.
