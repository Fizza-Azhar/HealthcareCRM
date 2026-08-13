using HealthcareCRM.API.Data;
using HealthcareCRM.API.Models;
using HealthcareCRM.API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace HealthcareCRM.API.Controllers
{
    /// <summary>
    /// Manages doctor records. All endpoints require a valid Bearer token;
    /// deactivate/reactivate additionally require the Admin role.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the list of doctors. Inactive doctors are excluded by default.
        /// </summary>
        /// <param name="includeInactive">If true, includes deactivated doctors in the results.</param>
        /// <response code="200">Returns the matching doctors.</response>
        /// <response code="401">Missing or invalid Bearer token.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDoctors([FromQuery] bool includeInactive = false)
        {
            var query = _context.Doctors.AsQueryable();

            if (!includeInactive)
                query = query.Where(d => d.IsActive);

            var doctors = await query.ToListAsync();
            return Ok(ApiResponse<List<Doctor>>.Ok(doctors));
        }

        /// <summary>
        /// Returns a single doctor by ID, regardless of active status.
        /// </summary>
        /// <param name="id">The doctor's ID.</param>
        /// <response code="200">Returns the doctor.</response>
        /// <response code="404">No doctor exists with this ID.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
                return NotFound(ApiResponse<Doctor>.Fail("Doctor not found."));

            return Ok(ApiResponse<Doctor>.Ok(doctor));
        }

        /// <summary>
        /// Creates a new doctor record.
        /// </summary>
        /// <param name="doctor">Name, specialization, phone, and schedule days. All required.</param>
        /// <response code="201">Doctor created. Response includes the new doctor's ID.</response>
        /// <response code="400">Validation failed (missing/invalid field).</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDoctor(Doctor doctor)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid doctor data."));

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDoctor),
                new { id = doctor.Id },
                ApiResponse<Doctor>.Ok(doctor, "Doctor created successfully."));
        }

        /// <summary>
        /// Updates an existing doctor's details. Active status is not changed here —
        /// use the deactivate/reactivate endpoints for that.
        /// </summary>
        /// <param name="id">The doctor's ID.</param>
        /// <param name="updated">The new field values to apply.</param>
        /// <response code="200">Doctor updated successfully.</response>
        /// <response code="404">No doctor exists with this ID.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDoctor(int id, Doctor updated)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound(ApiResponse<string>.Fail("Doctor not found."));

            doctor.Name = updated.Name;
            doctor.Specialization = updated.Specialization;
            doctor.Phone = updated.Phone;
            doctor.ScheduleDays = updated.ScheduleDays;
            // IsActive is intentionally NOT updated here —
            // that's handled only by deactivate/reactivate below (no hard-delete rule)

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<Doctor>.Ok(doctor, "Doctor updated successfully."));
        }

        /// <summary>
        /// Deactivates a doctor (soft-delete — the record is kept, not removed). Admin only.
        /// </summary>
        /// <param name="id">The doctor's ID.</param>
        /// <response code="200">Doctor deactivated successfully.</response>
        /// <response code="400">Doctor is already inactive.</response>
        /// <response code="403">Caller does not have the Admin role.</response>
        /// <response code="404">No doctor exists with this ID.</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound(ApiResponse<string>.Fail("Doctor not found."));

            if (!doctor.IsActive)
                return BadRequest(ApiResponse<string>.Fail("Doctor is already inactive."));

            doctor.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Doctor>.Ok(doctor, "Doctor deactivated successfully."));
        }

        /// <summary>
        /// Reactivates a previously deactivated doctor. Admin only.
        /// </summary>
        /// <param name="id">The doctor's ID.</param>
        /// <response code="200">Doctor reactivated successfully.</response>
        /// <response code="400">Doctor is already active.</response>
        /// <response code="403">Caller does not have the Admin role.</response>
        /// <response code="404">No doctor exists with this ID.</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}/reactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reactivate(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound(ApiResponse<string>.Fail("Doctor not found."));

            if (doctor.IsActive)
                return BadRequest(ApiResponse<string>.Fail("Doctor is already active."));

            doctor.IsActive = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Doctor>.Ok(doctor, "Doctor reactivated successfully."));
        }
    }
}