# Healthcare CRM

## Project Overview

Healthcare CRM is a web application built with ASP.NET Core MVC for managing patients, doctors, and appointments in a small clinic setting. It's designed for two types of users: **Staff**, who handle day-to-day patient/doctor/appointment management, and **Admins**, who additionally get access to an analytics dashboard, user role management, and data export tools. Access is secured with JWT authentication and role-based permissions, so every action is checked against the logged-in user's role before it's allowed.

This README is written for someone who has never seen this codebase before. If you're picking this project up for the first time, follow the setup steps below in order and you should have it running locally within a few minutes.

## Prerequisites

Before you start, make sure you have the following installed:

- **.NET 8 SDK** (verify with `dotnet --version` — should show `8.0.x`)
- **SQL Server Express** (or another SQL Server edition — see note below if your instance name differs)
- **Git**
- A code editor (Visual Studio, VS Code, or similar)

> **Note on SQL Server:** This project's default configuration expects a local SQL Server instance named `SQLEXPRESS` (the standard name when you install SQL Server Express). If your SQL Server installation uses a different instance name, you'll need to update the connection string in `appsettings.json` (see Step 3 below) to match your instance.

## Setup — From Clone to Running

### 1. Clone the repository

```bash
git clone https://github.com/Fizza-Azhar/HealthcareCRM.git
cd HealthcareCRM
```

### 2. Restore packages

```bash
cd HealthcareCRM.API
dotnet restore
```

### 3. Configure your database connection

Open `HealthcareCRM.API/appsettings.json` and check the `ConnectionStrings` section:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=HealthcareCRM;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

If your SQL Server instance has a different name, replace `localhost\SQLEXPRESS` with your actual server/instance name. If you're unsure what your instance is called, open SQL Server Management Studio (SSMS) and check the name shown when you connect.

### 4. Run database migrations

This creates the `HealthcareCRM` database and all its tables automatically:

```bash
dotnet ef database update
```

If `dotnet ef` isn't recognized, install it first with:
```bash
dotnet tool install --global dotnet-ef
```

### 5. Run the project

```bash
dotnet run
```

You should see output ending with something like:
```
Now listening on: http://localhost:5100
```

Open that URL in your browser.

### 6. Log in

On first startup, the app automatically creates a default Admin account if one doesn't already exist (no manual database editing required). Use these credentials to log in:

| Field | Value |
|---|---|
| Email | `admin@healthcarecrm.com` |
| Password | `Admin@123` |

**Important:** Change this password (or create a new Admin account and deactivate this one) before using the app beyond local development/testing, since this default password is publicly visible in this README.

There is no automatic seed data for sample Patients, Doctors, or Appointments — you'll need to add these manually through the app's UI after logging in, or via the API directly.

## Architecture Overview

The project is split into two parts:

```
HealthcareCRM
├── HealthcareCRM.API          — the main web application
│   ├── Controllers/           — API endpoints (one file per resource: Patients, Doctors, Appointments, Users, Auth, Export, Dashboard)
│   ├── Models/                — database entity classes (Patient, Doctor, Appointment, User, AuditLog)
│   ├── DTOs/                  — request/response shapes used by the API (e.g. RegisterRequest, UpdateRoleDto)
│   ├── Data/                  — AppDbContext (the Entity Framework database context)
│   ├── Middleware/             — global exception handling
│   ├── Helpers/                — JWT token generation logic
│   ├── Views/                  — MVC pages (Login, Register, Patient/Doctor/Appointment lists and forms, Dashboard, User Management)
│   ├── Migrations/             — Entity Framework database migration history
│   ├── wwwroot/                — static files (CSS, images)
│   └── Program.cs              — application startup: services, authentication, middleware pipeline, and Admin seeding
└── HealthcareCRM.Tests         — the automated test project (xUnit)
```

**How a request flows through the app:** Browser → MVC View (renders HTML) → JavaScript `fetch()` call → API Controller → Entity Framework → SQL Server, with the JWT token attached to every API call after login to prove who's making the request.

**MVC vs. API layers:** Controllers ending in `Controller` under routes like `/api/patients` are pure API endpoints returning JSON. Controllers like `AccountController` and `UserController` (no "s") serve the actual HTML pages via Views.

## Default Credentials

| Role | Email | Password |
|---|---|---|
| Admin (auto-created on first run) | `admin@healthcarecrm.com` | `Admin@123` |

Additional Staff accounts can be created via the Register page. Any newly registered user starts as **Staff**; an existing Admin must promote them via the User Management page (`/User`) to grant Admin access.

## Features

- User Registration with hashed password storage and server-side validation (required fields, email format, minimum password length)
- User Login with JWT token issuance (role-aware post-login redirect: Admin → Dashboard, Staff → Home)
- Deactivated user accounts are blocked at login, independent of valid credentials
- Role-Based Access Control (RBAC): Admin and Staff roles, enforced via an `AdminOnly` authorization policy
- Protected API endpoints (Doctors, Patients, Appointments, Users, Dashboard, Export) requiring a valid Bearer token
- Patient Management (Full CRUD, soft-delete + reactivate), with per-field inline validation
- Doctor Management (Full CRUD, soft-delete + reactivate), with per-field inline validation
- Appointment Management (booking with past-date validation, status filtering, confirm/cancel, permanent delete)
- Analytics Dashboard (Admin-only): live metric cards and appointment status breakdown chart, with graceful empty-state handling
- User Management (Admin-only): view all users and their active status, promote/demote roles, activate/deactivate accounts, all changes recorded to an audit log
- Data Export (Admin-only): CSV export of active patients, PDF export of appointment reports with a date range filter
- Home page with live stats for any authenticated user
- Global exception-handling middleware returning structured, consistent error responses with correct HTTP status codes
- Automated test suite (xUnit) — 79+ passing tests

## Technologies Used

- ASP.NET Core 8 MVC
- C#
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication
- QuestPDF (PDF report generation)
- xUnit, Microsoft.EntityFrameworkCore.InMemory, Microsoft.AspNetCore.Mvc.Testing, Moq (testing)
- Chart.js (dashboard visualization)
- HTML, CSS, JavaScript (Fetch API)
- Swagger / Swashbuckle

## API Response Standard

All API endpoints return a consistent shape:
```json
{
  "success": true,
  "data": { },
  "message": "Success"
}
```
Unhandled exceptions are caught globally and return the same shape with `success: false`, using the correct HTTP status code for the type of error (400 for bad input, 404 for not found, 403 for unauthorized, 500 for anything unexpected).

## API Endpoints

### Authentication
- `POST /api/auth/register`
- `POST /api/auth/login`

### Patients (requires auth; deactivate/reactivate require Admin)
- `GET /api/patients` — supports `search`, `page`, `pageSize`, `includeInactive`
- `GET /api/patients/{id}`
- `POST /api/patients`
- `PUT /api/patients/{id}`
- `PUT /api/patients/{id}/deactivate` (Admin only)
- `PUT /api/patients/{id}/reactivate` (Admin only)

### Doctors (requires auth; deactivate/reactivate require Admin)
- `GET /api/doctors` — supports `includeInactive`
- `GET /api/doctors/{id}`
- `POST /api/doctors`
- `PUT /api/doctors/{id}`
- `PUT /api/doctors/{id}/deactivate` (Admin only)
- `PUT /api/doctors/{id}/reactivate` (Admin only)

### Appointments (requires auth)
- `GET /api/appointments` — supports `status`
- `GET /api/appointments/{id}`
- `POST /api/appointments`
- `PUT /api/appointments/{id}`
- `PUT /api/appointments/{id}/status`
- `DELETE /api/appointments/{id}`

### Users (Admin only)
- `GET /api/users`
- `PUT /api/users/{id}/role`
- `PUT /api/users/{id}/toggle-active`

### Export (Admin only)
- `GET /api/export/patients/export?format=csv`
- `GET /api/export/appointments/report?from=&to=`

### Dashboard (Admin only)
- `GET /api/dashboard/stats`

### Home Stats (any authenticated user)
- `GET /api/home-stats`

## Swagger

With the project running, open `http://localhost:5100/swagger` in your browser. To test protected endpoints:
1. Log in via `POST /api/auth/login` (either through Swagger itself or the app's login page) and copy the returned token
2. Click the **Authorize** button (top right of the Swagger page)
3. Paste just the token (no `Bearer` prefix — Swagger adds it automatically)
4. Try any endpoint directly from the browser

## Testing

Run the full test suite with:
```bash
cd HealthcareCRM.Tests
dotnet test
```

The test project contains 79+ tests covering Auth, Patients, Doctors, Appointments, Dashboard, Users, Export, RBAC (verified via real HTTP requests, not just direct controller calls), and the global exception middleware.

## Known Issues / Carry-Forward Items

- **Pagination queries lack explicit ordering.** The Patients list query and the appointment date-range export query use `Skip`/`Take` without an `OrderBy`, which EF Core flags as a warning — results could theoretically be returned in an inconsistent order across repeated calls. Not yet fixed; low risk in current usage but worth addressing.
- **No automatic seed data for sample Patients/Doctors/Appointments.** Only the default Admin account is auto-created. A fresh setup starts with an empty clinic dataset.
- **Default Admin password is publicly documented in this README.** Anyone deploying this beyond local development should change it immediately after first login.

## Documentation

Additional documentation, under `HealthcareCRM.API/Documentation/`:
- Entity Relationship Diagram (ERD)
- Bug Tracking Sheet
- Authentication, Patient, Doctor, Appointment, Dashboard, and RBAC Test Cases

## Developed By
Fizza Azhar
Backend Developer Intern