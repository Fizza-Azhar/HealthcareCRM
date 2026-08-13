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
    /// <summary>
    /// Provides data export endpoints (CSV, PDF). All endpoints require the Admin role
    /// and cap results at 500 records per request.
    /// </summary>
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

        /// <summary>
        /// Exports the list of currently active patients as a downloadable CSV file.
        /// </summary>
        /// <param name="format">Export format. Only "csv" is currently supported.</param>
        /// <response code="200">Returns the CSV file (up to 500 rows).</response>
        /// <response code="400">An unsupported format was requested.</response>
        /// <response code="403">Caller does not have the Admin role.</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("patients/export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Generates a PDF report of appointments within an optional date range, including
        /// patient name, doctor name, date, and status for each appointment. Defaults to the
        /// last month if no range is provided.
        /// </summary>
        /// <param name="from">Optional. Start of the date range (inclusive). Defaults to one month before today.</param>
        /// <param name="to">Optional. End of the date range (inclusive). Defaults to today.</param>
        /// <response code="200">Returns the PDF file (up to 500 appointment rows).</response>
        /// <response code="400">The 'from' date is after the 'to' date.</response>
        /// <response code="403">Caller does not have the Admin role.</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("appointments/report")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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