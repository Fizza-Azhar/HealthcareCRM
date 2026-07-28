using HealthcareCRM.API.Controllers;
using HealthcareCRM.API.Data;
using HealthcareCRM.API.DTOs;
using HealthcareCRM.API.Helpers;
using HealthcareCRM.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthcareCRM.Tests
{
    public class DashboardControllerTests
    {
        private DashboardController CreateController(out AppDbContext context)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            context = new AppDbContext(options);
            return new DashboardController(context);
        }

        [Fact]
        public async Task GetStats_EmptyDatabase_ReturnsZeroes()
        {
            var controller = CreateController(out var context);

            var result = await controller.GetStats();

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<DashboardStatsDto>>(ok.Value);
            Assert.Equal(0, response.Data!.TotalPatients);
            Assert.Equal(0, response.Data.PendingCount);
        }

        [Fact]
        public async Task GetStats_WithActivePatients_ReturnsCorrectCount()
        {
            var controller = CreateController(out var context);
            context.Patients.AddRange(
                new Patient { FirstName = "A", LastName = "B", Age = 1, Gender = "M", PhoneNumber = "1", Email = "a@t.com", IsActive = true },
                new Patient { FirstName = "C", LastName = "D", Age = 2, Gender = "F", PhoneNumber = "2", Email = "c@t.com", IsActive = false }
            );
            await context.SaveChangesAsync();

            var result = await controller.GetStats();

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<DashboardStatsDto>>(ok.Value);
            Assert.Equal(1, response.Data!.TotalPatients);
        }

        [Fact]
        public async Task GetStats_WithPendingAppointments_ReturnsCorrectPendingCount()
        {
            var controller = CreateController(out var context);
            var patient = new Patient { FirstName = "A", LastName = "B", Age = 1, Gender = "M", PhoneNumber = "1", Email = "a@t.com", IsActive = true };
            var doctor = new Doctor { Name = "Dr. X", Specialization = "Y", Phone = "1", ScheduleDays = "Mon", IsActive = true };
            context.Patients.Add(patient);
            context.Doctors.Add(doctor);
            await context.SaveChangesAsync();

            context.Appointments.Add(new Appointment { PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Now, Status = "Pending" });
            await context.SaveChangesAsync();

            var result = await controller.GetStats();

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<DashboardStatsDto>>(ok.Value);
            Assert.Equal(1, response.Data!.PendingCount);
        }

        [Fact]
        public async Task GetStats_StatusBreakdown_GroupsCorrectly()
        {
            var controller = CreateController(out var context);
            var patient = new Patient { FirstName = "A", LastName = "B", Age = 1, Gender = "M", PhoneNumber = "1", Email = "a@t.com", IsActive = true };
            var doctor = new Doctor { Name = "Dr. X", Specialization = "Y", Phone = "1", ScheduleDays = "Mon", IsActive = true };
            context.Patients.Add(patient);
            context.Doctors.Add(doctor);
            await context.SaveChangesAsync();

            context.Appointments.AddRange(
                new Appointment { PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Now, Status = "Confirmed" },
                new Appointment { PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Now, Status = "Confirmed" },
                new Appointment { PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Now, Status = "Cancelled" }
            );
            await context.SaveChangesAsync();

            var result = await controller.GetStats();

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<DashboardStatsDto>>(ok.Value);
            Assert.Equal(2, response.Data!.StatusBreakdown["Confirmed"]);
            Assert.Equal(1, response.Data.StatusBreakdown["Cancelled"]);
        }

        [Fact]
        public async Task GetStats_AppointmentsToday_CountsOnlyTodaysAppointments()
        {
            var controller = CreateController(out var context);
            var patient = new Patient { FirstName = "A", LastName = "B", Age = 1, Gender = "M", PhoneNumber = "1", Email = "a@t.com", IsActive = true };
            var doctor = new Doctor { Name = "Dr. X", Specialization = "Y", Phone = "1", ScheduleDays = "Mon", IsActive = true };
            context.Patients.Add(patient);
            context.Doctors.Add(doctor);
            await context.SaveChangesAsync();

            context.Appointments.AddRange(
                new Appointment { PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Today.AddHours(10), Status = "Pending" },
                new Appointment { PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Today.AddDays(5), Status = "Pending" }
            );
            await context.SaveChangesAsync();

            var result = await controller.GetStats();

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<DashboardStatsDto>>(ok.Value);
            Assert.Equal(1, response.Data!.AppointmentsToday);
        }
    }
}