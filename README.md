# Healthcare CRM

## Project Overview
Healthcare CRM is a web application developed using ASP.NET Core MVC. It allows staff to manage patients, doctors, and appointments through a role-aware interface, with secured API access via JWT authentication and Role-Based Access Control (RBAC). The application includes login/registration, full CRUD for Patients and Doctors, a complete Appointment booking and management module, an Admin-only Analytics Dashboard, user/role management, and SQL Server database integration via Entity Framework Core.

## Features

- User Registration with hashed password storage
- User Login with JWT token issuance (role-aware post-login redirect: Admin → Dashboard, Staff → Home)
- Role-Based Access Control (RBAC): Admin and Staff roles, enforced via an `AdminOnly` authorization policy
- Protected API endpoints (Doctors, Patients, Appointments, Users, Dashboard) requiring a valid Bearer token
- Patient Management (Full CRUD, soft-delete + reactivate)
  - Add Patient
  - View Patient List (search, pagination, "show inactive" toggle for Admins)
  - View Patient Details (full profile)
  - Edit Patient
  - Deactivate / Reactivate Patient (Admin-only, no permanent removal)
- Doctor Management (Full CRUD, soft-delete + reactivate)
  - Add Doctor
  - View Doctor List (search, "show inactive" toggle for Admins)
  - Edit Doctor
  - Deactivate / Reactivate Doctor (Admin-only, no hard-delete)
- Appointment Management (complete end-to-end)
  - Book Appointment (select patient, doctor, date/time, notes)
  - View Appointment List with status filter (Pending / Confirmed / Cancelled)
  - Confirm or Cancel appointment (with confirmation dialog)
  - Foreign key validation — no orphan appointments
- Analytics Dashboard (Admin-only)
  - Live metric cards: total patients, appointments today/this week, pending count
  - Appointment status breakdown chart (Chart.js)
  - No hardcoded values — all figures pulled live from the database
- User Management (Admin-only)
  - View all users and their roles
  - Promote Staff to Admin / demote Admin to Staff
- Home page with live stats (active doctors, registered patients, appointments this week) via a lightweight non-admin stats endpoint
- Shared navigation layout across all authenticated pages, with Admin-only links (Dashboard, User Management) hidden from Staff
- Global exception-handling middleware — every unhandled error returns a structured `{ success: false, message, data: null }` response instead of a raw crash
- SQL Server Database Integration via Entity Framework Core
- Swagger API Testing (with Bearer token authorization support)
- Automated test suite (xUnit) — 45+ passing tests covering Auth, Patients, Doctors, Appointments, and Dashboard modules
- Project Documentation (test cases, bug tracker, ERD)

## Technologies Used

- ASP.NET Core 8 MVC
- C#
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication (Microsoft.AspNetCore.Authentication.JwtBearer)
- xUnit + Microsoft.EntityFrameworkCore.InMemory (automated testing)
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
└── xUnit test project (Controllers unit tests)
## Database

**Database Name:** `HealthcareCRM`

### Tables

**Users**
- Id, FirstName, LastName, Email, PasswordHash, Role (Staff / Admin)

**Patients**
- Id, FirstName, LastName, Age, Gender, PhoneNumber, Email, IsActive

**Doctors**
- Id, Name, Specialization, Phone, ScheduleDays, IsActive

**Appointments**
- Id, PatientId (FK), DoctorId (FK), DateTime, Status, Notes

## API Response Standard

All API endpoints return a consistent shape:
```json
{
  "success": true,
  "data": { },
  "message": "Success"
}
```
Unhandled exceptions are caught globally and return the same shape with `success: false`.

## Authentication & Authorization

- Registration hashes passwords using ASP.NET's `PasswordHasher<T>` — plain text passwords are never stored
- Login verifies credentials and issues a signed JWT (HMAC SHA-256), containing user id, email, name, and role claims
- Token is stored in the browser's Local Storage and sent as `Authorization: Bearer {token}` on every protected request
- All API controllers require a valid token (`[Authorize]`)
- Admin-only actions (Dashboard, User management, Doctor/Patient deactivate & reactivate) additionally require the `AdminOnly` policy (`RequireRole("Admin")`)
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

### Users (Admin only)
- `GET /api/users`
- `PUT /api/users/{id}/role`

### Dashboard (Admin only)
- `GET /api/dashboard/stats`

### Home Stats (any authenticated user)
- `GET /api/home-stats`

## UI

A shared layout (`Views/Shared/_Layout.cshtml`) provides a persistent top navigation bar across all authenticated pages, with Admin-only links hidden for Staff. Login and Register pages render standalone without the navbar. The application uses a consistent green/beige design system — shared color palette, card-based layouts, pill-style buttons, avatar-initial badges, and centered forms — defined in `wwwroot/css/site.css`, `patient.css`, `login.css`, and `register.css`. The Home page features a hero section, trust/rating row, feature strip, live stats bar, and about section, all using licensed images stored under `wwwroot/assets/Images`.

## Testing

The `HealthcareCRM.Tests` project contains 45+ xUnit tests using EF Core's InMemory provider, covering:
- Auth: registration, duplicate email, valid/invalid login
- Patients: CRUD, search, pagination, invalid IDs, deactivate/reactivate
- Doctors: CRUD, deactivate/reactivate
- Appointments: booking, foreign key validation, status transitions
- Dashboard: live stat accuracy across empty and populated data sets

RBAC (401/403 enforcement) is verified manually and documented in `Documentation/RBACTestCases.pdf`.

Run tests with:
cd HealthcareCRM.Tests
dotnet test
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