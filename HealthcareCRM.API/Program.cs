using HealthcareCRM.API.Data;
using Microsoft.EntityFrameworkCore;
using HealthcareCRM.API.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token (just the token itself, no 'Bearer' prefix needed — Swagger adds it automatically)"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// Register SQL Server Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<JwtHelper>();

var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
         options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse(); // stop default behavior
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Authentication required. Please log in.",
                    data = (object?)null
                });
                await context.Response.WriteAsync(result);
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "You do not have permission to access this resource.",
                    data = (object?)null
                });
                await context.Response.WriteAsync(result);
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate(); // ensures database + tables exist

    var adminExists = context.Users.Any(u => u.Role == "Admin");
    if (!adminExists)
    {
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<HealthcareCRM.API.Models.User>();
        var defaultAdmin = new HealthcareCRM.API.Models.User
        {
            FirstName = "Default",
            LastName = "Admin",
            Email = "admin@healthcarecrm.com",
            Role = "Admin",
            IsActive = true
        };
        defaultAdmin.PasswordHash = hasher.HashPassword(defaultAdmin, "Admin@123");

        context.Users.Add(defaultAdmin);
        context.SaveChanges();
    }
}
app.UseMiddleware<HealthcareCRM.API.Middleware.ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// MVC Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API Controllers
app.MapControllers();

// Health API
app.MapGet("/api/health", () =>
{
    return Results.Ok("Healthcare CRM API is running successfully.");
})
.WithName("HealthCheck")
.WithOpenApi();

app.Run();
public partial class Program { }