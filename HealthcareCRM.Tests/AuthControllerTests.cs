using HealthcareCRM.API.Controllers;
using HealthcareCRM.API.Data;
using HealthcareCRM.API.Helpers;
using HealthcareCRM.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HealthcareCRM.Tests
{
    public class AuthControllerTests
    {
        private AuthController CreateController(out AppDbContext context)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            context = new AppDbContext(options);

            var configValues = new Dictionary<string, string?>
            {
                { "Jwt:Key", "TestSecretKeyForUnitTests_MustBeLongEnough123!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpiryMinutes", "60" }
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            var jwtHelper = new JwtHelper(config);

            return new AuthController(context, jwtHelper);
        }

        [Fact]
        public async Task Register_NewUser_ReturnsOkAndCreatesUser()
        {
            var controller = CreateController(out var context);
            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "newuser@example.com",
                Password = "Password123"
            };

            var result = await controller.Register(request);

            Assert.IsType<OkObjectResult>(result);
            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
            Assert.NotNull(userInDb);
            Assert.NotEqual("Password123", userInDb!.PasswordHash);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            var controller = CreateController(out var context);
            context.Users.Add(new User
            {
                FirstName = "Existing",
                LastName = "User",
                Email = "duplicate@example.com",
                PasswordHash = "somehash"
            });
            await context.SaveChangesAsync();

            var request = new RegisterRequest
            {
                FirstName = "New",
                LastName = "User",
                Email = "duplicate@example.com",
                Password = "Password123"
            };

            var result = await controller.Register(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            var controller = CreateController(out var context);
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            var user = new User
            {
                FirstName = "Valid",
                LastName = "User",
                Email = "valid@example.com"
            };
            user.PasswordHash = hasher.HashPassword(user, "CorrectPassword1");
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var request = new LoginRequest { Email = "valid@example.com", Password = "CorrectPassword1" };

            var result = await controller.Login(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            var controller = CreateController(out var context);
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            var user = new User
            {
                FirstName = "Valid",
                LastName = "User",
                Email = "valid2@example.com"
            };
            user.PasswordHash = hasher.HashPassword(user, "CorrectPassword1");
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var request = new LoginRequest { Email = "valid2@example.com", Password = "WrongPassword" };

            var result = await controller.Login(request);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_NonExistentEmail_ReturnsUnauthorized()
        {
            var controller = CreateController(out var context);

            var request = new LoginRequest { Email = "doesnotexist@example.com", Password = "Whatever123" };

            var result = await controller.Login(request);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
        [Fact]
public async Task Register_MissingFirstName_StillCreatesUser_NoServerValidation()
{
    // Exposes a real gap: AuthController.Register doesn't check ModelState.IsValid
    var controller = CreateController(out var context);
    var request = new RegisterRequest
    {
        FirstName = "",
        LastName = "User",
        Email = "nofirstname@example.com",
        Password = "Password123"
    };

    var result = await controller.Register(request);

    // Documents actual current behavior — if this fails after you add validation, that's expected and good
    Assert.IsType<OkObjectResult>(result);
}

[Fact]
public async Task Register_EmptyPassword_HashesEmptyStringInsteadOfRejecting()
{
    var controller = CreateController(out var context);
    var request = new RegisterRequest
    {
        FirstName = "Test",
        LastName = "User",
        Email = "emptypass@example.com",
        Password = ""
    };

    var result = await controller.Register(request);

    Assert.IsType<OkObjectResult>(result);
    var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Email == "emptypass@example.com");
    Assert.NotNull(userInDb);
}

[Fact]
public async Task Login_EmptyEmail_ReturnsUnauthorized()
{
    var controller = CreateController(out var context);
    var request = new LoginRequest { Email = "", Password = "Whatever123" };

    var result = await controller.Login(request);

    Assert.IsType<UnauthorizedObjectResult>(result);
}

[Fact]
public async Task Login_EmptyPassword_ReturnsUnauthorized()
{
    var controller = CreateController(out var context);
    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
    var user = new User { FirstName = "Test", LastName = "User", Email = "emptypwlogin@example.com" };
    user.PasswordHash = hasher.HashPassword(user, "RealPassword123");
    context.Users.Add(user);
    await context.SaveChangesAsync();

    var request = new LoginRequest { Email = "emptypwlogin@example.com", Password = "" };

    var result = await controller.Login(request);

    Assert.IsType<UnauthorizedObjectResult>(result);
}

[Fact]
public async Task Register_ReturnsExactSuccessMessage()
{
    var controller = CreateController(out var context);
    var request = new RegisterRequest
    {
        FirstName = "Msg",
        LastName = "Check",
        Email = "msgcheck@example.com",
        Password = "Password123"
    };

    var result = await controller.Register(request);

    var ok = Assert.IsType<OkObjectResult>(result);
    var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
    Assert.Contains("Registration successful", json);
}
    }
}