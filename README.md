# Healthcare CRM

## Project Overview
Healthcare CRM is a web application developed using ASP.NET Core MVC. It allows staff to manage patients, doctors, and appointments through a simple, user-friendly interface, with secured API access via JWT authentication. The application includes login/registration, full CRUD for Patients and Doctors, a complete Appointment booking and management module, and SQL Server database integration via Entity Framework Core.

## Features

- User Registration with hashed password storage
- User Login with JWT token issuance
- Protected API endpoints (Doctors, Patients, Appointments) requiring a valid Bearer token
- Patient Management (Full CRUD, soft-delete)
  - Add Patient
  - View Patient List (search, pagination)
  - View Patient Details (full profile)
  - Edit Patient
  - Deactivate Patient (soft-delete — no permanent removal)
- Doctor Management (Full CRUD, soft-delete)
  - Add Doctor
  - View Doctor List (active doctors, with search)
  - Edit Doctor
  - Deactivate / Reactivate Doctor (no hard-delete)
- Appointment Management (complete end-to-end)
  - Book Appointment (select patient, doctor, date/time, notes)
  - View Appointment List with status filter (Pending / Confirmed / Cancelled)
  - Confirm or Cancel appointment (with confirmation dialog)
  - Foreign key validation — no orphan appointments
- Shared navigation layout across all authenticated pages
- SQL Server Database Integration via Entity Framework Core
- Swagger API Testing (with Bearer token authorization support)
- Project Documentation (test cases, bug tracker, ERD)

## Technologies Used

- ASP.NET Core 8 MVC
- C#
- Entity Framework Core
- SQL Server
- JWT Bearer Authentication (Microsoft.AspNetCore.Authentication.JwtBearer)
- HTML, CSS, JavaScript (Fetch API)
- Swagger / Swashbuckle

## Project Structure
HealthcareCRM.API
│
├── Controllers
├── DTOs
├── Helpers
├── Models
├── Views
│   └── Shared (_Layout.cshtml)
├── Data
├── Migrations
├── Services
├── Repositories
├── wwwroot
├── Documentation
└── Program.cs
## Database

**Database Name:** `HealthcareCRM`

### Tables

**Users**
- Id, FirstName, LastName, Email, PasswordHash, Role

**Patients**
- Id, FirstName, LastName, Age, Gender, PhoneNumber, Email, IsActive

**Doctors**
- Id, Name, Specialization, Phone, ScheduleDays, IsActive

**Appointments**
- Id, PatientId (FK), DoctorId (FK), DateTime, Status, Notes

## API Response Standard

All Doctor, Patient, and Appointment API endpoints return a consistent shape:
```json
{
  "success": true,
  "data": { },
  "message": "Success"
}
```

## Authentication

- Registration hashes passwords using ASP.NET's `PasswordHasher<T>` — plain text passwords are never stored
- Login verifies credentials and issues a signed JWT (HMAC SHA-256), containing user id, email, name, and role claims
- Token is stored in the browser's Local Storage and sent as `Authorization: Bearer {token}` on every protected request
- Doctors, Patients, and Appointments API controllers all require a valid token (`[Authorize]`)

## API Endpoints

### Authentication
- `POST /api/auth/register`
- `POST /api/auth/login`

### Patients (requires auth)
- `GET /api/patients` — supports `search`, `page`, `pageSize`, `includeInactive` query params
- `GET /api/patients/{id}`
- `POST /api/patients`
- `PUT /api/patients/{id}`
- `PUT /api/patients/{id}/deactivate`

### Doctors (requires auth)
- `GET /api/doctors` — supports `includeInactive` query param
- `GET /api/doctors/{id}`
- `POST /api/doctors`
- `PUT /api/doctors/{id}`
- `PUT /api/doctors/{id}/deactivate`
- `PUT /api/doctors/{id}/reactivate`

### Appointments (requires auth)
- `GET /api/appointments` — supports `status` query param
- `GET /api/appointments/{id}`
- `POST /api/appointments`
- `PUT /api/appointments/{id}`
- `PUT /api/appointments/{id}/status`

## UI

A shared layout (`Views/Shared/_Layout.cshtml`) provides a persistent top navigation bar (Home, Patients, Doctors, Appointments, Logout) across all authenticated pages. Login and Register pages render standalone without the navbar. The application uses a consistent design system — shared color palette, card-based layouts, pill-style buttons, and centered forms — defined in `wwwroot/css/site.css` and `wwwroot/css/patient.css`.

## Documentation

The project includes, under `Documentation/`:
- Entity Relationship Diagram (ERD)
- Bug Tracking Sheet (updated through Week 3)
- Authentication Test Cases
- Patient CRUD Test Cases
- Doctor CRUD Test Cases (Week 3)
- Appointment Test Cases (Week 3)

## Developed By
Fizza Azhar
Backend Developer Intern