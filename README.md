# Healthcare CRM

## Project Overview

Healthcare CRM is a web application developed using ASP.NET Core MVC. It allows users to manage patient and doctor records through a simple and user-friendly interface. The application includes authentication pages, patient management (full CRUD), a doctor directory, and SQL Server database integration via Entity Framework Core.

---

## Features

- User Login
- User Registration
- Patient Management (Full CRUD)
  - Add Patient
  - View Patient List (with search and pagination)
  - View Patient Details (full profile)
  - Edit Patient
  - Delete Patient
- Doctor Directory (Read-only)
  - View Doctor List (active doctors only)
- SQL Server Database Integration
- Entity Framework Core
- Authentication Token stored in Local Storage
- Swagger API Testing
- Project Documentation

---

## Technologies Used

- ASP.NET Core 8 MVC
- C#
- Entity Framework Core
- SQL Server
- HTML
- CSS
- JavaScript (Fetch API)
- Swagger

---

## Project Structure

```
HealthcareCRM.API
│
├── Controllers
├── Models
├── Views
├── Data
├── Services
├── Repositories
├── wwwroot
├── Documentation
└── Program.cs
```

---

## Database

Database Name:

```
HealthcareCRM
```

Tables:

### Patients

- Id
- FirstName
- LastName
- Age
- Gender
- PhoneNumber
- Email

### Doctors

- Id
- Name
- Specialization
- Phone
- IsActive

---

## Patient CRUD Operations

- Create Patient
- Read Patient List (search by name/phone, paginated)
- Read Patient Details (full profile view)
- Update Patient
- Delete Patient

## Doctor Operations

- Read Doctor List (active doctors only; full CRUD planned for Week 3)

---

## Authentication

- Login Page
- Registration Page
- Email Validation
- Password Validation
- Token stored in Local Storage

---

## API Endpoints

### Authentication

- POST `/api/auth/login`
- POST `/api/auth/register`

### Patients

- GET `/api/patients` — supports `search`, `page`, `pageSize` query params
- GET `/api/patients/{id}`
- POST `/api/patients`
- PUT `/api/patients/{id}`
- DELETE `/api/patients/{id}`

### Doctors

- GET `/api/doctors` — returns active doctors only

---

## UI

The application uses a consistent design system across all pages (login, register, patient management, and doctor directory): a shared color palette, card-based layouts, and centered forms, defined in `wwwroot/css/site.css` and `wwwroot/css/patient.css`.

---

## Documentation

The project includes:

- Entity Relationship Diagram (ERD)
- Bug Tracking Sheet
- Authentication Test Cases
- Patient CRUD Test Cases
- Doctor List Test Cases

---

## Developed By

**Fizza Azhar**

Backend Developer Intern