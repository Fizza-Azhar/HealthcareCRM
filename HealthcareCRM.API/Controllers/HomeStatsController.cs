using HealthcareCRM.API.Data;
using HealthcareCRM.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareCRM.API.Controllers
{
    /// <summary>
    /// Provides lightweight, live clinic statistics for the Home page.
    /// Available to any authenticated user (Staff or Admin) — unlike the Dashboard endpoint,
    /// this does not require the Admin role.
    /// </summary>
    [ApiController]
    [Route("api/home-stats")]
    [Authorize]
    public class HomeStatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HomeStatsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the count of active doctors, active patients, and appointments this week.
        /// </summary>
        /// <response code="200">Returns the current stats.</response>
        /// <response code="401">Missing or invalid Bearer token.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get()
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);

            var activeDoctors = await _context.Doctors.CountAsync(d => d.IsActive);
            var totalPatients = await _context.Patients.CountAsync(p => p.IsActive);
            var appointmentsThisWeek = await _context.Appointments
                .CountAsync(a => a.DateTime.Date >= startOfWeek && a.DateTime.Date < startOfWeek.AddDays(7));

            return Ok(ApiResponse<object>.Ok(new
            {
                activeDoctors,
                totalPatients,
                appointmentsThisWeek
            }));
        }
    }
}