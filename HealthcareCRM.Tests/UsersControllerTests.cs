using HealthcareCRM.API.Controllers;
using HealthcareCRM.API.Data;
using HealthcareCRM.API.DTOs;
using HealthcareCRM.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace HealthcareCRM.Tests
{
    public class UsersControllerTests
    {
        private UsersController CreateController(out AppDbContext context, int adminId = 1)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            context = new AppDbContext(options);

            var controller = new UsersController(context);

            // Simulate an authenticated Admin's claims so LogAction can read the acting user's Id
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, adminId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            return controller;
        }

        private async Task<User> SeedUser(AppDbContext context, string role = "Staff", bool isActive = true)
        {
            var user = new User
            {
                FirstName = "Test",
                LastName = "User",
                Email = $"{Guid.NewGuid()}@test.com",
                PasswordHash = "hash",
                Role = role,
                IsActive = isActive
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        [Fact]
        public async Task GetUsers_ReturnsAllUsersWithStatus()
        {
            var controller = CreateController(out var context);
            await SeedUser(context, "Staff", true);
            await SeedUser(context, "Admin", false);

            var result = await controller.GetUsers();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task UpdateRole_ValidRole_UpdatesSuccessfully()
        {
            var controller = CreateController(out var context);
            var user = await SeedUser(context, "Staff");

            var result = await controller.UpdateRole(user.Id, new UpdateRoleDto { Role = "Admin" });

            Assert.IsType<OkObjectResult>(result);
            var updated = await context.Users.FindAsync(user.Id);
            Assert.Equal("Admin", updated!.Role);
        }

        [Fact]
        public async Task UpdateRole_InvalidRole_ReturnsBadRequest()
        {
            var controller = CreateController(out var context);
            var user = await SeedUser(context, "Staff");

            var result = await controller.UpdateRole(user.Id, new UpdateRoleDto { Role = "SuperAdmin" });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateRole_NonExistentUser_ReturnsNotFound()
        {
            var controller = CreateController(out var context);

            var result = await controller.UpdateRole(9999, new UpdateRoleDto { Role = "Admin" });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateRole_LogsAuditEntry()
        {
            var controller = CreateController(out var context, adminId: 7);
            var user = await SeedUser(context, "Staff");

            await controller.UpdateRole(user.Id, new UpdateRoleDto { Role = "Admin" });

            var log = await context.AuditLogs.FirstOrDefaultAsync();
            Assert.NotNull(log);
            Assert.Equal("RoleChanged", log!.Action);
            Assert.Equal(user.Id, log.TargetId);
            Assert.Equal(7, log.UserId);
        }

        [Fact]
        public async Task ToggleActive_ActiveUser_DeactivatesSuccessfully()
        {
            var controller = CreateController(out var context);
            var user = await SeedUser(context, isActive: true);

            var result = await controller.ToggleActive(user.Id);

            Assert.IsType<OkObjectResult>(result);
            var updated = await context.Users.FindAsync(user.Id);
            Assert.False(updated!.IsActive);
        }

        [Fact]
        public async Task ToggleActive_InactiveUser_ActivatesSuccessfully()
        {
            var controller = CreateController(out var context);
            var user = await SeedUser(context, isActive: false);

            var result = await controller.ToggleActive(user.Id);

            Assert.IsType<OkObjectResult>(result);
            var updated = await context.Users.FindAsync(user.Id);
            Assert.True(updated!.IsActive);
        }

        [Fact]
        public async Task ToggleActive_NonExistentUser_ReturnsNotFound()
        {
            var controller = CreateController(out var context);

            var result = await controller.ToggleActive(9999);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}