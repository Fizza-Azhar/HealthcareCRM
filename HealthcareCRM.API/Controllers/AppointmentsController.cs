using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthcareCRM.API.Data;
using HealthcareCRM.API.Models;
using HealthcareCRM.API.Helpers;
using HealthcareCRM.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace HealthcareCRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        private static readonly string[] ValidStatuses = { "Pending", "Confirmed", "Cancelled" };

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/appointments
        // GET: api/appointments?status=Confirmed
        [HttpGet]
        public async Task<IActionResult> GetAppointments([FromQuery] string? status)
        {
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (!ValidStatuses.Contains(status))
                    return BadRequest(ApiResponse<string>.Fail(
                        $"Invalid status filter. Must be one of: {string.Join(", ", ValidStatuses)}"));

                query = query.Where(a => a.Status == status);
            }

            var appointments = await query.ToListAsync();
            return Ok(ApiResponse<List<Appointment>>.Ok(appointments));
        }

        // GET: api/appointments/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound(ApiResponse<Appointment>.Fail("Appointment not found."));

            return Ok(ApiResponse<Appointment>.Ok(appointment));
        }

        // POST: api/appointments
        [HttpPost]
        public async Task<IActionResult> CreateAppointment(Appointment appointment)
        {
            var patientExists = await _context.Patients.AnyAsync(p => p.Id == appointment.PatientId);
            if (!patientExists)
                return BadRequest(ApiResponse<string>.Fail("Invalid PatientId — patient does not exist."));

            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == appointment.DoctorId);
            if (!doctorExists)
                return BadRequest(ApiResponse<string>.Fail("Invalid DoctorId — doctor does not exist."));

            if (string.IsNullOrEmpty(appointment.Status))
                appointment.Status = "Pending";
            else if (!ValidStatuses.Contains(appointment.Status))
                return BadRequest(ApiResponse<string>.Fail(
                    $"Invalid status. Must be one of: {string.Join(", ", ValidStatuses)}"));

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetAppointment),
                new { id = appointment.Id },
                ApiResponse<Appointment>.Ok(appointment, "Appointment created successfully."));
        }

        // PUT: api/appointments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, Appointment appointment)
        {
            if (id != appointment.Id)
                return BadRequest(ApiResponse<string>.Fail("Id in URL does not match Id in body."));

            var patientExists = await _context.Patients.AnyAsync(p => p.Id == appointment.PatientId);
            if (!patientExists)
                return BadRequest(ApiResponse<string>.Fail("Invalid PatientId — patient does not exist."));

            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == appointment.DoctorId);
            if (!doctorExists)
                return BadRequest(ApiResponse<string>.Fail("Invalid DoctorId — doctor does not exist."));

            var exists = await _context.Appointments.AnyAsync(a => a.Id == id);
            if (!exists)
                return NotFound(ApiResponse<string>.Fail("Appointment not found."));

            _context.Entry(appointment).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Appointment>.Ok(appointment, "Appointment updated successfully."));
        }

        // PUT: api/appointments/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            if (!ValidStatuses.Contains(dto.Status))
                return BadRequest(ApiResponse<string>.Fail(
                    $"Invalid status. Must be one of: {string.Join(", ", ValidStatuses)}"));

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(ApiResponse<string>.Fail("Appointment not found."));

            appointment.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Appointment>.Ok(appointment, "Status updated successfully."));
        }
        // DELETE: api/appointments/5
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteAppointment(int id)
{
    var appointment = await _context.Appointments.FindAsync(id);
    if (appointment == null)
        return NotFound(ApiResponse<string>.Fail("Appointment not found."));

    _context.Appointments.Remove(appointment);
    await _context.SaveChangesAsync();

    return Ok(ApiResponse<string>.Ok(null!, "Appointment deleted successfully."));
}
    }
}