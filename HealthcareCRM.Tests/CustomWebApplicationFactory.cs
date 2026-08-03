using System.Linq;
using HealthcareCRM.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HealthcareCRM.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // NOTE: we do NOT override Jwt:Key/Issuer/Audience here.
            // Program.cs reads those values into local variables before
            // app.Build() runs, so a config override added here arrives
            // too late to affect token signing/validation. Instead, tests
            // read the REAL Jwt settings from the running app's own
            // configuration (see RbacTests.cs) and sign test tokens with
            // those same real values.

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("RbacTestDb_" + Guid.NewGuid()));
            });
        }
    }
}