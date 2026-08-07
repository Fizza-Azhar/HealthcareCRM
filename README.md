# Healthcare CRM

## Project Overview
Healthcare CRM is a web application developed using ASP.NET Core MVC. It allows staff to manage patients, doctors, and appointments through a role-aware interface, with secured API access via JWT authentication and Role-Based Access Control (RBAC). The application includes login/registration, full CRUD for Patients and Doctors, a complete Appointment booking and management module, an Admin-only Analytics Dashboard, full user/role/status management with audit logging, CSV/PDF data export, and SQL Server database integration via Entity Framework Core.

## Features

- User Registration with hashed password storage and server-side validation (required fields, email format, minimum password length)
- User Login with JWT token issuance (role-aware post-login redirect: Admin → Dashboard, Staff → Home)
- Deactivated user accounts are blocked at login, independent of valid credentials
- Role-Based Access Control (RBAC): Admin and Staff roles, enforced via an `AdminOnly` authorization policy
- Protected API endpoints (Doctors, Patients, Appointments, Users, Dashboard, Export) requiring a valid Bearer token
- Patient Management (Full CRUD, soft-delete + reactivate)
  - Add Patient
  - View Patient List (search, pagination, "show inactive" toggle for Admins)
  - View Patient Details (full profile)
  - Edit Patient
  - Deactivate / Reactivate Patient (Admin-only, no permanent removal)
  - Per-field inline validation with server-side error display
- Doctor Management (Full CRUD, soft-delete + reactivate)
  - Add Doctor
  - View Doctor List (search, "show inactive" toggle for Admins)
  - Edit Doctor
  - Deactivate / Reactivate Doctor (Admin-only, no hard-delete)
  - Per-field inline validation with server-side error display
- Appointment Management (complete end-to-end)
  - Book Appointment (select patient, doctor, date/time, notes), with past-date validation
  - View Appointment List with status filter (Pending / Confirmed / Cancelled)
  - Confirm or Cancel appointment (with confirmation dialog)
  - Delete appointment permanently (with confirmation dialog)
  - Foreign key validation — no orphan appointments
- Analytics Dashboard (Admin-only)
  - Live metric cards: total patients, appointments today/this week, pending count
  - Appointment status breakdown chart (Chart.js), with a graceful empty-state message when no appointment data exists
  - No hardcoded values — all figures pulled live from the database
- User Management (Admin-only)
  - View all users, their roles, and active/inactive status
  - Promote Staff to Admin / demote Admin to Staff
  - Activate / deactivate user accounts (deactivated users cannot log in)
  - Every role change and status change is recorded in an audit log (who did what, to whom, and when)
- Data Export (Admin-only)
  - Export active patient list to CSV
  - Export appointment report to PDF, with a configurable date range, generated via QuestPDF
  - Both export actions capped at 500 records per request
- Home page with live stats (active doctors, registered patients, appointments this week) via a lightweight non-admin stats endpoint
- Shared navigation layout across all authenticated pages, with Admin-only links (Dashboard, User Management) hidden from Staff
- Global exception-handling middleware — every unhandled error returns a structured `{ success: false, message, data: null }` response instead of a raw crash, with distinct HTTP status codes for validation errors (400), not-found (404), and unauthorized (403) exceptions in addition to generic 500s
- Empty-state handling on list screens and the dashboard chart — no blank or broken UI on zero-record datasets
- SQL Server Database Integration via Entity Framework Core
- Swagger API Testing (with Bearer token authorization support)
- Automated test suite (xUnit) — 79+ passing tests covering Auth, Patients, Doctors, Appointments, Dashboard, Users, Export, RBAC, and the global exception middleware
- Project Documentation (test cases, bug tracker, ERD)

## Technologies Used

- ASP.NET Core 8 MVC
- C#
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication (Microsoft.AspNetCore.Authentication.JwtBearer)
- QuestPDF (PDF report generation)
- xUnit + Microsoft.EntityFrameworkCore.InMemory + Microsoft.AspNetCore.Mvc.Testing + Moq (automated unit and integration testing)
- Chart.js (Dashboard visualization)
- HTML, CSS, JavaScript (Fetch API)
- Swagger / Swashbuckle

## Project Structure
HealthcareCRM
├── HealthcareCRM.API
│ ├── Controllers
│ ├── DTOs
│ ├── Helpers
│ ├── Middleware
│ ├── Models
│ ├── Views
│ │ └── Shared (_Layout.cshtml)
│ ├── Data
│ ├── Migrations
│ ├── wwwroot
│ ├── Documentation
│ └── Program.cs
└── HealthcareCRM.Tests
├── Unit tests (Controllers)
├── Middleware tests
└── Integration tests (RBAC, WebApplicationFactory-based)

## Database

**Database Name:** `HealthcareCRM`

### Tables

**Users**
- Id, FirstName, LastName, Email, PasswordHash, Role (Staff / Admin), IsActive

**Patients**
- Id, FirstName, LastName, Age, Gender, PhoneNumber, Email, IsActive

**Doctors**
- Id, Name, Specialization, Phone, ScheduleDays, IsActive

**Appointments**
- Id, PatientId (FK), DoctorId (FK), DateTime, Status, Notes

**AuditLogs**
- Id, UserId (admin who performed the action), Action, TargetId (user affected), Timestamp

## API Response Standard

All API endpoints return a consistent shape:
```json
{
  "success": true,
  "data": { },
  "message": "Success"
}
```
Unhandled exceptions are caught globally and return the same shape with `success: false`. The global handler distinguishes exception types to return the correct status code:
- `ArgumentException` → 400
- `KeyNotFoundException` → 404
- `UnauthorizedAccessException` → 403
- anything else → 500, with a generic message (no internal exception details are leaked to the client)

## Authentication & Authorization

- Registration hashes passwords using ASP.NET's `PasswordHasher<T>` — plain text passwords are never stored
- Registration and login both validate required fields, email format, and minimum password length server-side (`DataAnnotations` + `ModelState` checks), in addition to client-side per-field validation
- Login verifies credentials and issues a signed JWT (HMAC SHA-256), containing user id, email, name, and role claims
- Login is blocked for deactivated accounts (`IsActive = false`), even with correct credentials
- Token is stored in the browser's Local Storage and sent as `Authorization: Bearer {token}` on every protected request
- All API controllers require a valid token (`[Authorize]`)
- Admin-only actions (Dashboard, User management, Doctor/Patient deactivate & reactivate, Export) additionally require the `AdminOnly` policy (`RequireRole("Admin")`)
- Unauthenticated and unauthorized requests return structured JSON (`401`/`403` with `{ success: false, message, data: null }`) via custom `JwtBearerEvents`, rather than bare status codes
- Frontend reads the role from the decoded JWT to conditionally show/hide Admin-only navigation links; server-side policy enforcement is the actual security boundary

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
- `GET /api/users` — includes active/inactive status
- `PUT /api/users/{id}/role`
- `PUT /api/users/{id}/toggle-active`

### Export (Admin only)
- `GET /api/export/patients/export?format=csv`
- `GET /api/export/appointments/report?from=&to=`

### Dashboard (Admin only)
- `GET /api/dashboard/stats`

### Home Stats (any authenticated user)
- `GET /api/home-stats`

## UI

A shared layout (`Views/Shared/_Layout.cshtml`) provides a persistent top navigation bar across all authenticated pages, with Admin-only links hidden for Staff. Login and Register pages render standalone without the navbar. The application uses a consistent green/beige design system — shared color palette, card-based layouts, pill-style buttons, avatar-initial badges, and centered forms — defined in `wwwroot/css/site.css`, `patient.css`, `login.css`, and `register.css`. The Home page features a hero section, trust/rating row, feature strip, live stats bar, and about section, all using licensed images stored under `wwwroot/assets/Images`.

All forms (Login, Register, Patient, Doctor, Appointment) share a common pattern:
- Per-field inline validation errors, displayed next to the relevant input rather than in a single shared message
- Submit buttons disabled with a loading label while a request is in flight, preventing double-submission
- A friendly "Could not connect to the server" message on network failure, instead of a silent failure or unhandled promise rejection

## Testing

The `HealthcareCRM.Tests` project contains 79+ xUnit tests, covering:
- Auth: registration, duplicate email, valid/invalid login, missing-field and format validation, deactivated-account login block
- Patients: CRUD, search, pagination, invalid IDs, deactivate/reactivate
- Doctors: CRUD, deactivate/reactivate
- Appointments: booking, foreign key validation, status transitions
- Dashboard: live stat accuracy across empty and populated data sets
- Users: list, role change (valid/invalid), toggle active/inactive, audit log entry correctness
- Export: CSV export with/without data, PDF export with valid/invalid date ranges, unsupported format handling
- Global exception middleware: correct status code per exception type, generic message on unhandled errors
- RBAC: Admin-only routes correctly return 200/403/401 depending on the caller's role and token, verified via real HTTP requests against an in-process test server (`Microsoft.AspNetCore.Mvc.Testing`), not just direct controller calls

Run tests with:
```
cd HealthcareCRM.Tests
dotnet test
```

## Documentation

The project includes, under `HealthcareCRM.API/Documentation/`:
- Entity Relationship Diagram (ERD)
- Bug Tracking Sheet (updated through Week 4)
- Authentication Test Cases
- Patient CRUD Test Cases
- Doctor CRUD Test Cases
- Appointment Test Cases
- Dashboard Test Cases
- RBAC Test Cases

## Developed By
Fizza Azhar
Backend Developer Intern