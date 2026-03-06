# Fionetix Employee Registry — Backend API

ASP.NET Core 10 Web API for the Fionetix Employee & Family Registry. Manages employee profiles, spouse and children records, role-based access control via Firebase Authentication, and PDF report generation.

**Live API URL:** https://fionetix-task-server.onrender.com

**OpenAPI Docs:** https://fionetix-task-server.onrender.com/openapi/v1.json

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 Web API |
| Database | PostgreSQL 14+ |
| ORM | Entity Framework Core 10 + Npgsql |
| Validation | FluentValidation 11 |
| Authentication | Firebase Admin SDK 3 (JWT verification) |
| PDF Generation | QuestPDF 2026 |
| Containerization | Docker |
| Deployment | Render (Docker runtime) |

---

## Features

### Employee Management
- Full CRUD for employee records: name, NID (10 or 17 digits), phone (Bangladesh format), department, basic salary
- NID uniqueness enforced at both application and database level (409 Conflict on duplicate)
- `UpdatedAt` auto-stamped on every save via `SaveChangesAsync` override

### Family Data
- **Spouse** — one-to-one relationship; upsert endpoint creates or updates in place; NID optional
- **Children** — one-to-many relationship; supports add, update, and delete per child
- Cascade delete: removing an employee automatically removes all linked spouse and children records

### Search
- `GET /api/employees?search=term` — case-insensitive partial match on name, NID, and department using PostgreSQL `ILIKE`
- Results sorted alphabetically by name

### Role-Based Access Control
- Two roles: **Admin** (full CRUD) and **Viewer** (read-only)
- Every mutating endpoint checks `HttpContext.Items["UserRole"]`; Viewer requests receive HTTP 403
- Roles are stored in the `AppUsers` table, seeded on first run

### Firebase Authentication
- Every request (except `/openapi`) must carry a valid `Authorization: Bearer <Firebase JWT>` header
- `FirebaseAuthMiddleware` verifies the token using the Firebase Admin SDK and attaches `UserId`, `UserEmail`, and `UserRole` to the request context
- Unknown Firebase/Google users are automatically registered as `Viewer` on their first request
- If Firebase is not initialized, middleware returns HTTP 401 with a descriptive message instead of crashing
- Dev bypass: in Development mode or when `ENABLE_DEV_AUTH=true`, an `X-Dev-Email` header can be used instead of a Firebase token

### PDF Export
- **Table PDF** (`GET /api/export/pdf-list?search=`) — generates a formatted table PDF of the currently filtered employee list including header, search filter label, and total count footer
- **CV PDF** (`GET /api/export/cv/{id}`) — generates a per-employee CV PDF with personal details, spouse information, and a children table with ages calculated at generation time

### Seed Data
- On first run, `SeedData` inserts 10 realistic Bangladeshi employees with varied departments, 6 spouses, 10 children, and 2 `AppUsers` (Admin + Viewer) if tables are empty

---

## Project Structure

```
server/
└── FionetixAPI/
    ├── Controllers/
    │   ├── AuthController.cs         # GET /api/auth/me
    │   ├── EmployeesController.cs    # Employee + spouse + children CRUD
    │   └── ExportController.cs       # PDF table + CV export
    ├── Data/
    │   ├── AppDbContext.cs           # EF Core context with index and cascade config
    │   └── SeedData.cs              # 10 employees + users seeded on startup
    ├── DTOs/
    │   ├── EmployeeDto.cs           # Create/Update/Response DTOs
    │   ├── SpouseDto.cs
    │   └── ChildDto.cs
    ├── Middleware/
    │   └── FirebaseAuthMiddleware.cs # JWT verification + auto user registration
    ├── Migrations/                   # EF Core migration history
    ├── Models/
    │   ├── Employee.cs
    │   ├── Spouse.cs
    │   ├── Child.cs
    │   └── AppUser.cs
    ├── Services/
    │   ├── EmployeeService.cs        # Business logic, search, CRUD
    │   └── PdfService.cs            # QuestPDF table and CV generation
    ├── Validators/
    │   ├── CreateEmployeeValidator.cs # NID, phone, salary, spouse, children rules
    │   ├── SpouseValidator.cs
    │   └── ChildValidator.cs
    ├── Dockerfile
    ├── Program.cs                    # DI, middleware pipeline, CORS, Firebase init
    ├── appsettings.json
    └── appsettings.Development.json
```

---

## API Endpoints

All endpoints require `Authorization: Bearer <Firebase JWT>` unless noted.

### Auth
| Method | Route | Description |
|---|---|---|
| GET | `/api/auth/me` | Returns current user's email and role |

### Employees
| Method | Route | Role | Description |
|---|---|---|---|
| GET | `/api/employees` | Any | List all; supports `?search=` |
| GET | `/api/employees/{id}` | Any | Single employee with spouse + children |
| POST | `/api/employees` | Admin | Create employee |
| PUT | `/api/employees/{id}` | Admin | Update employee |
| DELETE | `/api/employees/{id}` | Admin | Delete employee (cascades) |

### Spouse
| Method | Route | Role | Description |
|---|---|---|---|
| PUT | `/api/employees/{id}/spouse` | Admin | Upsert spouse |
| DELETE | `/api/employees/{id}/spouse` | Admin | Remove spouse |

### Children
| Method | Route | Role | Description |
|---|---|---|---|
| POST | `/api/employees/{id}/children` | Admin | Add a child |
| PUT | `/api/employees/{id}/children/{childId}` | Admin | Update a child |
| DELETE | `/api/employees/{id}/children/{childId}` | Admin | Remove a child |

### Export
| Method | Route | Role | Description |
|---|---|---|---|
| GET | `/api/export/pdf-list` | Any | PDF of filtered employee table |
| GET | `/api/export/cv/{id}` | Any | PDF CV of a single employee |

---

## Validation Rules

| Field | Rule |
|---|---|
| Employee NID | Required; exactly 10 or 17 digits; unique across all employees |
| Employee Phone | Required; must match `^(\+8801[3-9]\d{8}\|01[3-9]\d{8})$` |
| Employee Name | Required; max 200 characters |
| Employee Department | Required; max 100 characters |
| Employee BasicSalary | Required; must be greater than 0 |
| Spouse Name | Required when a spouse record is submitted |
| Spouse NID | Optional; if provided, must be exactly 10 or 17 digits |
| Child Name | Required |
| Child DateOfBirth | Required; must not be in the future |

---

## Prerequisites

- .NET 10 SDK
- PostgreSQL 14+
- Firebase project (for token verification in production)

---

## Local Development Setup

### 1. Clone and restore

```bash
cd server/FionetixAPI
dotnet restore
```

### 2. Configure the database connection

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=fionetix_db;Username=postgres;Password=your_password"
  },
  "Firebase": {
    "ProjectId": "your-firebase-project-id"
  }
}
```

### 3. Run migrations

```bash
dotnet ef database update
```

This creates all tables and applies indexes. Seed data is inserted automatically on first app startup.

### 4. Start the API

```bash
dotnet run
```

API runs at `http://localhost:5000`. OpenAPI JSON is available at `http://localhost:5000/openapi/v1.json`.

### 5. Development authentication bypass

In development mode, you can call the API without a Firebase token using the `X-Dev-Email` header:

```bash
curl http://localhost:5000/api/auth/me \
  -H "X-Dev-Email: admin@fionetix.com"
```

The email must match a record in the `AppUsers` table (seeded automatically).

---

## Running with Docker

```bash
cd server/FionetixAPI
docker build -t fionetix-api .
docker run -p 5000:8080 \
  -e DATABASE_URL="Host=host.docker.internal;Database=fionetix_db;Username=postgres;Password=your_password" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e Firebase__ProjectId=your-firebase-project-id \
  -e FIREBASE_SERVICE_ACCOUNT_JSON='{"type":"service_account",...}' \
  fionetix-api
```

---

## Environment Variables (Production / Render)

| Variable | Description |
|---|---|
| `DATABASE_URL` | PostgreSQL connection string (provided by Render from linked DB) |
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` |
| `Firebase__ProjectId` | Firebase project ID for token verification |
| `FIREBASE_SERVICE_ACCOUNT_JSON` | Full Firebase service account JSON string |
| `ALLOWED_ORIGINS` | Comma-separated list of allowed CORS origins |
| `ENABLE_DEV_AUTH` | Set to `true` to allow `X-Dev-Email` bypass on deployed instances (testing only) |

---

## Deploying to Render

The `render.yaml` at the repository root defines the complete Render infrastructure:
- A managed PostgreSQL database (`fionetix-db`)
- A Docker web service (`fionetix-api`) built from `server/FionetixAPI/Dockerfile`

Push to the connected GitHub branch to trigger an automatic redeploy.

---

## Seeded Test Accounts

| Role | Email | Password (Firebase) |
|---|---|---|
| Admin | admin@fionetix.com | Admin@123 |
| Viewer | viewer@fionetix.com | Viewer@123 |

These accounts must exist in Firebase Authentication and their UIDs must match the `AppUsers` table. The seed inserts placeholder UIDs that are updated automatically on first login.
