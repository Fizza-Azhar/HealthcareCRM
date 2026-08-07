using HealthcareCRM.API.Controllers;
using HealthcareCRM.API.Data;
using HealthcareCRM.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthcareCRM.Tests
{
    public class ExportControllerTests
    {
        static ExportControllerTests()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        private ExportController CreateController(out AppDbContext context)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            context = new AppDbContext(options);
            return new ExportController(context);
        }

        [Fact]
        public async Task ExportPatients_WithData_ReturnsCsvFile()
        {
            var controller = CreateController(out var context);
            context.Patients.Add(new Patient
            {
                FirstName = "Alice",
                LastName = "Smith",
                Age = 30,
                Gender = "Female",
                PhoneNumber = "111",
                Email = "alice@test.com",
                IsActive = true
            });
            await context.SaveChangesAsync();

            var result = await controller.ExportPatients("csv");

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", file.ContentType);
            var content = System.Text.Encoding.UTF8.GetString(file.FileContents);
            Assert.Contains("Alice", content);
            Assert.Contains("Id,FirstName,LastName,Age,Gender,Phone,Email", content);
        }

        [Fact]
        public async Task ExportPatients_NoActivePatients_ReturnsHeaderOnlyCsv()
        {
            var controller = CreateController(out var context);
            context.Patients.Add(new Patient
            {
                FirstName = "Inactive",
                LastName = "Person",
                Age = 40,
                Gender = "Male",
                PhoneNumber = "222",
                Email = "inactive@test.com",
                IsActive = false
            });
            await context.SaveChangesAsync();

            var result = await controller.ExportPatients("csv");

            var file = Assert.IsType<FileContentResult>(result);
            var content = System.Text.Encoding.UTF8.GetString(file.FileContents);
            Assert.Contains("Id,FirstName,LastName,Age,Gender,Phone,Email", content);
            Assert.DoesNotContain("Inactive", content);
        }

        [Fact]
        public async Task ExportPatients_UnsupportedFormat_ReturnsBadRequest()
        {
            var controller = CreateController(out var context);

            var result = await controller.ExportPatients("xlsx");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ExportAppointmentReport_ValidRange_ReturnsPdfFile()
        {
            var controller = CreateController(out var context);
            var patient = new Patient { FirstName = "A", LastName = "B", Age = 1, Gender = "M", PhoneNumber = "1", Email = "a@t.com", IsActive = true };
            var doctor = new Doctor { Name = "Dr. X", Specialization = "Y", Phone = "1", ScheduleDays = "Mon", IsActive = true };
            context.Patients.Add(patient);
            context.Doctors.Add(doctor);
            await context.SaveChangesAsync();

            context.Appointments.Add(new Appointment
            {
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                DateTime = DateTime.Today,
                Status = "Confirmed"
            });
            await context.SaveChangesAsync();

            var result = await controller.ExportAppointmentReport(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", file.ContentType);
            Assert.True(file.FileContents.Length > 0);
        }

        [Fact]
        public async Task ExportAppointmentReport_InvalidRange_ReturnsBadRequest()
        {
            var controller = CreateController(out var context);

            var result = await controller.ExportAppointmentReport(DateTime.Today, DateTime.Today.AddDays(-5));

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ExportAppointmentReport_NoAppointmentsInRange_StillReturnsValidPdf()
        {
            var controller = CreateController(out var context);

            var result = await controller.ExportAppointmentReport(
                DateTime.Today.AddYears(-5), DateTime.Today.AddYears(-4));

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", file.ContentType);
            Assert.True(file.FileContents.Length > 0);
        }
    }
}