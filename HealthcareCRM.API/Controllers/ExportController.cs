using HealthcareCRM.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace HealthcareCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const int MaxRecords = 500;

        public ExportController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("patients/export")]
        public async Task<IActionResult> ExportPatients(string format = "csv")
        {
            if (format.ToLower() != "csv")
                return BadRequest(new { success = false, message = "Only 'csv' format is currently supported." });

            var patients = await _context.Patients
                .Where(p => p.IsActive)
                .Take(MaxRecords)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,FirstName,LastName,Age,Gender,Phone,Email");
            foreach (var p in patients)
            {
                sb.AppendLine($"{p.Id},{p.FirstName},{p.LastName},{p.Age},{p.Gender},{p.PhoneNumber},{p.Email}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "patients_export.csv");
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("appointments/report")]
public async Task<IActionResult> ExportAppointmentReport(DateTime? from, DateTime? to)
{
    var start = from ?? DateTime.Today.AddMonths(-1);
    var end = to ?? DateTime.Today;

    if (start > end)
        return BadRequest(new { success = false, message = "'from' date must be before 'to' date." });

    var appointments = await _context.Appointments
        .Include(a => a.Patient)
        .Include(a => a.Doctor)
        .Where(a => a.DateTime.Date >= start.Date && a.DateTime.Date <= end.Date)
        .Take(MaxRecords)
        .ToListAsync();

    var pdfBytes = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Margin(30);

            page.Header().Text($"Appointment Report: {start:yyyy-MM-dd} to {end:yyyy-MM-dd}")
                .FontSize(16).Bold();

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Text("Patient").Bold();
                    header.Cell().Text("Doctor").Bold();
                    header.Cell().Text("Date").Bold();
                    header.Cell().Text("Status").Bold();
                });

                foreach (var a in appointments)
                {
                    table.Cell().Text($"{a.Patient?.FirstName} {a.Patient?.LastName}");
                    table.Cell().Text(a.Doctor?.Name ?? "");
                    table.Cell().Text(a.DateTime.ToString("yyyy-MM-dd"));
                    table.Cell().Text(a.Status);
                }
            });

            page.Footer().Text($"Total appointments: {appointments.Count}").FontSize(10);
        });
    }).GeneratePdf();

    return File(pdfBytes, "application/pdf", "appointment_report.pdf");
}
    }
}