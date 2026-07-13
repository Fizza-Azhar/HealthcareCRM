using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthcareCRM.API.Data;
using HealthcareCRM.API.Models;

namespace HealthcareCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToListAsync();
        }

        // GET: api/appointments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            return appointment;
        }

        // POST: api/appointments
        [HttpPost]
        public async Task<ActionResult<Appointment>> CreateAppointment(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetAppointment),
                new { id = appointment.Id },
                appointment);
        }

        // PUT: api/appointments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, Appointment appointment)
        {
            if (id != appointment.Id)
                return BadRequest();

            _context.Entry(appointment).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}