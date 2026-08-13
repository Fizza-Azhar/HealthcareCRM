using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthcareCRM.API.Data;
using HealthcareCRM.API.Models;
using HealthcareCRM.API.Helpers;
using HealthcareCRM.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace HealthcareCRM.API.Controllers
{
    /// <summary>
    /// Manages appointment booking, status updates, and deletion. All endpoints require a valid Bearer token.
    /// </summary>
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

        /// <summary>
        /// Returns all appointments, optionally filtered by status. Includes patient and doctor details.
        /// </summary>
        /// <param name="status">Optional. One of: Pending, Confirmed, Cancelled.</param>
        /// <response code="200">Returns the matching appointments.</response>
        /// <response code="400">The status filter is not one of the valid values.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Returns a single appointment by ID, including patient and doctor details.
        /// </summary>
        /// <param name="id">The appointment's ID.</param>
        /// <response code="200">Returns the appointment.</response>
        /// <response code="404">No appointment exists with this ID.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Books a new appointment. Defaults to "Pending" status if none is provided.
        /// </summary>
        /// <param name="appointment">PatientId, DoctorId, DateTime, and optional Notes. Both PatientId and DoctorId must reference existing records.</param>
        /// <response code="201">Appointment created. Response includes the new appointment's ID.</response>
        /// <response code="400">Invalid PatientId/DoctorId, or an invalid status was provided.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Updates an existing appointment's full details (patient, doctor, date/time, notes).
        /// </summary>
        /// <param name="id">The appointment's ID (must match the ID in the request body).</param>
        /// <param name="appointment">The full updated appointment object.</param>
        /// <response code="200">Appointment updated successfully.</response>
        /// <response code="400">ID mismatch, or invalid PatientId/DoctorId.</response>
        /// <response code="404">No appointment exists with this ID.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Updates only the status of an appointment (e.g. confirming or cancelling it).
        /// </summary>
        /// <param name="id">The appointment's ID.</param>
        /// <param name="dto">The new status. One of: Pending, Confirmed, Cancelled.</param>
        /// <response code="200">Status updated successfully.</response>
        /// <response code="400">The status value is not one of the valid options.</response>
        /// <response code="404">No appointment exists with this ID.</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Permanently deletes an appointment. This cannot be undone.
        /// </summary>
        /// <param name="id">The appointment's ID.</param>
        /// <response code="200">Appointment deleted successfully.</response>
        /// <response code="404">No appointment exists with this ID.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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