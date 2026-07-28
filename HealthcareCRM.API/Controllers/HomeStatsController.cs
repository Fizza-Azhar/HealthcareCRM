using HealthcareCRM.API.Data;
using HealthcareCRM.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareCRM.API.Controllers
{
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

        [HttpGet]
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