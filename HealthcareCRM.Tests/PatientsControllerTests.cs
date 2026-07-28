using HealthcareCRM.API.Controllers;
using HealthcareCRM.API.Data;
using HealthcareCRM.API.Helpers;
using HealthcareCRM.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthcareCRM.Tests
{
    public class PatientsControllerTests
    {
        private PatientsController CreateController(out AppDbContext context)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            context = new AppDbContext(options);
            return new PatientsController(context);
        }

        private async Task SeedPatients(AppDbContext context)
        {
            context.Patients.AddRange(
                new Patient { FirstName = "Alice", LastName = "Smith", Age = 30, Gender = "Female", PhoneNumber = "111", Email = "alice@test.com", IsActive = true },
                new Patient { FirstName = "Bob", LastName = "Jones", Age = 45, Gender = "Male", PhoneNumber = "222", Email = "bob@test.com", IsActive = true },
                new Patient { FirstName = "Carol", LastName = "White", Age = 60, Gender = "Female", PhoneNumber = "333", Email = "carol@test.com", IsActive = false }
            );
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetPatients_Default_ReturnsOnlyActivePatients()
        {
            var controller = CreateController(out var context);
            await SeedPatients(context);

            var result = await controller.GetPatients(null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<Patient>>>(ok.Value);
            Assert.Equal(2, response.Data!.Count);
            Assert.All(response.Data, p => Assert.True(p.IsActive));
        }

        [Fact]
        public async Task GetPatients_IncludeInactive_ReturnsAllPatients()
        {
            var controller = CreateController(out var context);
            await SeedPatients(context);

            var result = await controller.GetPatients(null, includeInactive: true);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<Patient>>>(ok.Value);
            Assert.Equal(3, response.Data!.Count);
        }

        [Fact]
        public async Task GetPatients_SearchByFirstName_ReturnsMatchingPatients()
        {
            var controller = CreateController(out var context);
            await SeedPatients(context);

            var result = await controller.GetPatients("Alice");

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<Patient>>>(ok.Value);
            Assert.Single(response.Data!);
            Assert.Equal("Alice", response.Data![0].FirstName);
        }

        [Fact]
        public async Task GetPatients_SearchNoMatch_ReturnsEmptyList()
        {
            var controller = CreateController(out var context);
            await SeedPatients(context);

            var result = await controller.GetPatients("NonExistentName");

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<Patient>>>(ok.Value);
            Assert.Empty(response.Data!);
        }

        [Fact]
        public async Task GetPatients_Pagination_ReturnsCorrectPageSize()
        {
            var controller = CreateController(out var context);
            for (int i = 1; i <= 5; i++)
            {
                context.Patients.Add(new Patient { FirstName = $"P{i}", LastName = "Test", Age = 20 + i, Gender = "Other", PhoneNumber = $"{i}", Email = $"p{i}@test.com", IsActive = true });
            }
            await context.SaveChangesAsync();

            var result = await controller.GetPatients(null, page: 1, pageSize: 2);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<Patient>>>(ok.Value);
            Assert.Equal(2, response.Data!.Count);
        }

        [Fact]
        public async Task GetPatient_ValidId_ReturnsPatient()
        {
            var controller = CreateController(out var context);
            await SeedPatients(context);
            var existing = context.Patients.First();

            var result = await controller.GetPatient(existing.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<Patient>>(ok.Value);
            Assert.Equal(existing.Id, response.Data!.Id);
        }

        [Fact]
        public async Task GetPatient_InvalidId_ReturnsNotFound()
        {
            var controller = CreateController(out var context);

            var result = await controller.GetPatient(9999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreatePatient_ValidData_CreatesPatientSuccessfully()
        {
            var controller = CreateController(out var context);
            var patient = new Patient { FirstName = "New", LastName = "Patient", Age = 25, Gender = "Male", PhoneNumber = "999", Email = "new@test.com" };

            var result = await controller.CreatePatient(patient);

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(1, await context.Patients.CountAsync());
        }

        [Fact]
        public async Task CreatePatient_InvalidModelState_ReturnsBadRequest()
        {
            var controller = CreateController(out var context);
            controller.ModelState.AddModelError("FirstName", "Required");
            var patient = new Patient { LastName = "NoFirstName", Age = 25, Gender = "Male", PhoneNumber = "999", Email = "bad@test.com" };

            var result = await controller.CreatePatient(patient);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdatePatient_ValidId_UpdatesFieldsSuccessfully()
        {
            var controller = CreateController(out var context);
            await SeedPatients(context);
            var existing = context.Patients.First();

            var updated = new Patient { FirstName = "Updated", LastName = "Name", Age = 99, Gender = "Other", PhoneNumber = "000", Email = "updated@test.com" };
            var result = await controller.UpdatePatient(existing.Id, updated);

            Assert.IsType<OkObjectResult>(result);
            var patientInDb = await context.Patients.FindAsync(existing.Id);
            Assert.Equal("Updated", patientInDb!.FirstName);
        }

        [Fact]
        public async Task UpdatePatient_InvalidId_ReturnsNotFound()
        {
            var controller = CreateController(out var context);
            var updated = new Patient { FirstName = "X", LastName = "Y", Age = 1, Gender = "Other", PhoneNumber = "0", Email = "x@test.com" };

            var result = await controller.UpdatePatient(9999, updated);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Deactivate_ActivePatient_SetsInactiveSuccessfully()
        {
            var controller = CreateController(out var context);
            await SeedPatients(context);
            var active = context.Patients.First(p => p.IsActive);

            var result = await controller.Deactivate(active.Id);

            Assert.IsType<OkObjectResult>(result);
            var patientInDb = await context.Patients.FindAsync(active.Id);
            Assert.False(patientInDb!.IsActive);
        }

        [Fact]
        public async Task Deactivate_AlreadyInactivePatient_ReturnsBadRequest()
        {
            var controller = CreateController(out var context);
            await SeedPatients(context);
            var inactive = context.Patients.First(p => !p.IsActive);

            var result = await controller.Deactivate(inactive.Id);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Deactivate_InvalidId_ReturnsNotFound()
        {
            var controller = CreateController(out var context);

            var result = await controller.Deactivate(9999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Reactivate_InactivePatient_SetsActiveSuccessfully()
        {
            var controller = CreateController(out var context);
            await SeedPatients(context);
            var inactive = context.Patients.First(p => !p.IsActive);

            var result = await controller.Reactivate(inactive.Id);

            Assert.IsType<OkObjectResult>(result);
            var patientInDb = await context.Patients.FindAsync(inactive.Id);
            Assert.True(patientInDb!.IsActive);
        }

        [Fact]
        public async Task Reactivate_AlreadyActivePatient_ReturnsBadRequest()
        {
            var controller = CreateController(out var context);
            await SeedPatients(context);
            var active = context.Patients.First(p => p.IsActive);

            var result = await controller.Reactivate(active.Id);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}