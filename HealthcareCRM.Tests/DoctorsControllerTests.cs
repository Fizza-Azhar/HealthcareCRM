using HealthcareCRM.API.Controllers;
using HealthcareCRM.API.Data;
using HealthcareCRM.API.Helpers;
using HealthcareCRM.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthcareCRM.Tests
{
    public class DoctorsControllerTests
    {
        private DoctorsController CreateController(out AppDbContext context)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            context = new AppDbContext(options);
            return new DoctorsController(context);
        }

        private async Task SeedDoctors(AppDbContext context)
        {
            context.Doctors.AddRange(
                new Doctor { Name = "Dr. Ahmed Ali", Specialization = "Cardiology", Phone = "111", ScheduleDays = "Mon,Wed", IsActive = true },
                new Doctor { Name = "Dr. Sara Khan", Specialization = "Pediatrics", Phone = "222", ScheduleDays = "Tue,Thu", IsActive = false }
            );
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetDoctors_Default_ReturnsOnlyActive()
        {
            var controller = CreateController(out var context);
            await SeedDoctors(context);

            var result = await controller.GetDoctors();

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<Doctor>>>(ok.Value);
            Assert.Single(response.Data!);
        }

        [Fact]
        public async Task GetDoctors_IncludeInactive_ReturnsAll()
        {
            var controller = CreateController(out var context);
            await SeedDoctors(context);

            var result = await controller.GetDoctors(includeInactive: true);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<Doctor>>>(ok.Value);
            Assert.Equal(2, response.Data!.Count);
        }

        [Fact]
        public async Task GetDoctor_ValidId_ReturnsDoctor()
        {
            var controller = CreateController(out var context);
            await SeedDoctors(context);
            var existing = context.Doctors.First();

            var result = await controller.GetDoctor(existing.Id);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetDoctor_InvalidId_ReturnsNotFound()
        {
            var controller = CreateController(out var context);

            var result = await controller.GetDoctor(9999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateDoctor_ValidData_CreatesSuccessfully()
        {
            var controller = CreateController(out var context);
            var doctor = new Doctor { Name = "Dr. New", Specialization = "ENT", Phone = "333", ScheduleDays = "Fri" };

            var result = await controller.CreateDoctor(doctor);

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(1, await context.Doctors.CountAsync());
        }

        [Fact]
        public async Task UpdateDoctor_ValidId_UpdatesSuccessfully()
        {
            var controller = CreateController(out var context);
            await SeedDoctors(context);
            var existing = context.Doctors.First();

            var updated = new Doctor { Name = "Dr. Updated", Specialization = "Neurology", Phone = "999", ScheduleDays = "Mon" };
            var result = await controller.UpdateDoctor(existing.Id, updated);

            Assert.IsType<OkObjectResult>(result);
            var inDb = await context.Doctors.FindAsync(existing.Id);
            Assert.Equal("Dr. Updated", inDb!.Name);
        }

        [Fact]
        public async Task Deactivate_ActiveDoctor_SetsInactive()
        {
            var controller = CreateController(out var context);
            await SeedDoctors(context);
            var active = context.Doctors.First(d => d.IsActive);

            var result = await controller.Deactivate(active.Id);

            Assert.IsType<OkObjectResult>(result);
            var inDb = await context.Doctors.FindAsync(active.Id);
            Assert.False(inDb!.IsActive);
        }

        [Fact]
        public async Task Reactivate_InactiveDoctor_SetsActive()
        {
            var controller = CreateController(out var context);
            await SeedDoctors(context);
            var inactive = context.Doctors.First(d => !d.IsActive);

            var result = await controller.Reactivate(inactive.Id);

            Assert.IsType<OkObjectResult>(result);
            var inDb = await context.Doctors.FindAsync(inactive.Id);
            Assert.True(inDb!.IsActive);
        }
    }
}