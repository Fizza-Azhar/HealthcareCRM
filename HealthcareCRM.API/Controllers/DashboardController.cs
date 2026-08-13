using HealthcareCRM.API.Data;
using HealthcareCRM.API.DTOs;
using HealthcareCRM.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareCRM.API.Controllers
{
    /// <summary>
    /// Provides live analytics data for the Admin dashboard. Admin role required.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns live clinic statistics: total active patients, appointments today and this week,
        /// pending appointment count, and a breakdown of appointment counts by status.
        /// All figures are computed live from the database, never cached or hardcoded.
        /// </summary>
        /// <response code="200">Returns the current statistics.</response>
        /// <response code="403">Caller does not have the Admin role.</response>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetStats()
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);

            var totalPatients = await _context.Patients.CountAsync(p => p.IsActive);

            var appointmentsToday = await _context.Appointments
                .CountAsync(a => a.DateTime.Date == today);

            var appointmentsThisWeek = await _context.Appointments
                .CountAsync(a => a.DateTime.Date >= startOfWeek && a.DateTime.Date < startOfWeek.AddDays(7));

            var pendingCount = await _context.Appointments
                .CountAsync(a => a.Status == "Pending");

            var statusBreakdown = await _context.Appointments
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var stats = new DashboardStatsDto
            {
                TotalPatients = totalPatients,
                AppointmentsToday = appointmentsToday,
                AppointmentsThisWeek = appointmentsThisWeek,
                PendingCount = pendingCount,
                StatusBreakdown = statusBreakdown
            };

            return Ok(ApiResponse<DashboardStatsDto>.Ok(stats));
        }
    }
}