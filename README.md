# Healthcare CRM

## Project Overview

Healthcare CRM is a web application developed using ASP.NET Core MVC. It allows users to manage patient records through a simple and user-friendly interface. The application includes authentication pages, patient management (CRUD operations), and SQL Server database integration.

---

## Features

- User Login
- User Registration
- Patient Management (CRUD)
  - Add Patient
  - View Patient List
  - Edit Patient
  - Delete Patient
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

Main Table:

```
Patients
```

Columns:

- Id
- FirstName
- LastName
- Age
- Gender
- PhoneNumber
- Email

---

## Patient CRUD Operations

- Create Patient
- Read Patient List
- Update Patient
- Delete Patient

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

- GET `/api/patients`
- GET `/api/patients/{id}`
- POST `/api/patients`
- PUT `/api/patients/{id}`
- DELETE `/api/patients/{id}`

---

## Documentation

The project includes:

- Entity Relationship Diagram (ERD)
- Bug Tracking Sheet
- Authentication Test Cases
- Patient CRUD Test Cases

---

## Developed By

**Fizza Azhar**

Backend Developer Intern