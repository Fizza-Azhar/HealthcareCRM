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
    public class AppointmentsControllerTests
    {
        private AppointmentsController CreateController(out AppDbContext context)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            context = new AppDbContext(options);
            return new AppointmentsController(context);
        }

        private async Task<(Patient patient, Doctor doctor)> SeedBasics(AppDbContext context)
        {
            var patient = new Patient { FirstName = "Test", LastName = "Patient", Age = 30, Gender = "Male", PhoneNumber = "1", Email = "p@test.com", IsActive = true };
            var doctor = new Doctor { Name = "Dr. Test", Specialization = "General", Phone = "2", ScheduleDays = "Mon", IsActive = true };
            context.Patients.Add(patient);
            context.Doctors.Add(doctor);
            await context.SaveChangesAsync();
            return (patient, doctor);
        }

        [Fact]
        public async Task CreateAppointment_ValidData_CreatesSuccessfully()
        {
            var controller = CreateController(out var context);
            var (patient, doctor) = await SeedBasics(context);

            var appt = new Appointment { PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Now.AddDays(1), Notes = "Checkup" };
            var result = await controller.CreateAppointment(appt);

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(1, await context.Appointments.CountAsync());
        }

        [Fact]
        public async Task CreateAppointment_InvalidPatientId_ReturnsBadRequest()
        {
            var controller = CreateController(out var context);
            var (_, doctor) = await SeedBasics(context);

            var appt = new Appointment { PatientId = 9999, DoctorId = doctor.Id, DateTime = DateTime.Now.AddDays(1) };
            var result = await controller.CreateAppointment(appt);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateAppointment_InvalidDoctorId_ReturnsBadRequest()
        {
            var controller = CreateController(out var context);
            var (patient, _) = await SeedBasics(context);

            var appt = new Appointment { PatientId = patient.Id, DoctorId = 9999, DateTime = DateTime.Now.AddDays(1) };
            var result = await controller.CreateAppointment(appt);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateAppointment_DefaultsStatusToPending()
        {
            var controller = CreateController(out var context);
            var (patient, doctor) = await SeedBasics(context);

            var appt = new Appointment { PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Now.AddDays(1) };
            await controller.CreateAppointment(appt);

            var inDb = await context.Appointments.FirstAsync();
            Assert.Equal("Pending", inDb.Status);
        }

        [Fact]
        public async Task GetAppointments_FilterByStatus_ReturnsOnlyMatching()
        {
            var controller = CreateController(out var context);
            var (patient, doctor) = await SeedBasics(context);
            context.Appointments.AddRange(
                new Appointment { PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Now, Status = "Confirmed" },
                new Appointment { PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Now, Status = "Cancelled" }
            );
            await context.SaveChangesAsync();

            var result = await controller.GetAppointments("Confirmed");

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<Appointment>>>(ok.Value);
            Assert.Single(response.Data!);
        }

        [Fact]
        public async Task GetAppointments_InvalidStatusFilter_ReturnsBadRequest()
        {
            var controller = CreateController(out var context);

            var result = await controller.GetAppointments("BogusStatus");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetAppointment_InvalidId_ReturnsNotFound()
        {
            var controller = CreateController(out var context);

            var result = await controller.GetAppointment(9999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_ValidStatus_UpdatesSuccessfully()
        {
            var controller = CreateController(out var context);
            var (patient, doctor) = await SeedBasics(context);
            var appt = new Appointment { PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Now, Status = "Pending" };
            context.Appointments.Add(appt);
            await context.SaveChangesAsync();

            var result = await controller.UpdateStatus(appt.Id, new UpdateStatusDto { Status = "Confirmed" });

            Assert.IsType<OkObjectResult>(result);
            var inDb = await context.Appointments.FindAsync(appt.Id);
            Assert.Equal("Confirmed", inDb!.Status);
        }

        [Fact]
        public async Task UpdateStatus_InvalidStatus_ReturnsBadRequest()
        {
            var controller = CreateController(out var context);
            var (patient, doctor) = await SeedBasics(context);
            var appt = new Appointment { PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Now, Status = "Pending" };
            context.Appointments.Add(appt);
            await context.SaveChangesAsync();

            var result = await controller.UpdateStatus(appt.Id, new UpdateStatusDto { Status = "NotAValidStatus" });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_NonExistentAppointment_ReturnsNotFound()
        {
            var controller = CreateController(out var context);

            var result = await controller.UpdateStatus(9999, new UpdateStatusDto { Status = "Confirmed" });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAppointment_MismatchedId_ReturnsBadRequest()
        {
            var controller = CreateController(out var context);
            var (patient, doctor) = await SeedBasics(context);

            var appt = new Appointment { Id = 5, PatientId = patient.Id, DoctorId = doctor.Id, DateTime = DateTime.Now };
            var result = await controller.UpdateAppointment(999, appt);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}