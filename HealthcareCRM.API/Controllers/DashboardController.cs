using HealthcareCRM.API.Data;
using HealthcareCRM.API.DTOs;
using HealthcareCRM.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareCRM.API.Controllers
{
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

        [HttpGet("stats")]
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