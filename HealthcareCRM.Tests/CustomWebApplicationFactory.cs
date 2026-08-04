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
        // Generated ONCE per factory instance (constructor-time field init),
        // NOT inside ConfigureServices. WebApplicationFactory can invoke the
        // ConfigureWebHost/ConfigureServices callback more than once internally
        // (e.g. when .Services is accessed before CreateClient() builds the
        // real test server) — if the db name were generated inside that lambda,
        // each invocation would produce a different empty in-memory database,
        // meaning seeded data and the actual HTTP request could land in two
        // different databases. Fixing the name here guarantees every rebuild
        // of the host still points at the exact same in-memory database.
        private readonly string _dbName = "RbacTestDb_" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));
            });
        }
    }
}