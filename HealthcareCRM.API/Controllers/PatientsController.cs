using HealthcareCRM.API.Data;
using HealthcareCRM.API.Helpers;
using HealthcareCRM.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareCRM.API.Controllers
{
    /// <summary>
    /// Manages patient records. All endpoints require a valid Bearer token;
    /// deactivate/reactivate additionally require the Admin role.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PatientsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a paginated list of patients. Inactive patients are excluded by default.
        /// </summary>
        /// <param name="search">Optional. Filters by first name, last name, or phone number (partial match).</param>
        /// <param name="page">Page number, starting at 1. Defaults to 1.</param>
        /// <param name="pageSize">Number of results per page. Defaults to 20.</param>
        /// <param name="includeInactive">If true, includes deactivated patients in the results.</param>
        /// <response code="200">Returns the matching patients.</response>
        /// <response code="401">Missing or invalid Bearer token.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPatients(string? search, int page = 1, int pageSize = 20, bool includeInactive = false)
        {
            var query = _context.Patients.AsQueryable();

            if (!includeInactive)
                query = query.Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.FirstName.Contains(search) ||
                    p.LastName.Contains(search) ||
                    p.PhoneNumber.Contains(search));
            }

            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            var patients = await query.ToListAsync();
            return Ok(ApiResponse<List<Patient>>.Ok(patients));
        }

        /// <summary>
        /// Returns a single patient by ID, regardless of active status.
        /// </summary>
        /// <param name="id">The patient's ID.</param>
        /// <response code="200">Returns the patient.</response>
        /// <response code="404">No patient exists with this ID.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return NotFound(ApiResponse<Patient>.Fail("Patient not found."));

            return Ok(ApiResponse<Patient>.Ok(patient));
        }

        /// <summary>
        /// Creates a new patient record.
        /// </summary>
        /// <param name="patient">First name, last name, age, gender, phone number, and email. All required.</param>
        /// <response code="201">Patient created. Response includes the new patient's ID.</response>
        /// <response code="400">Validation failed (missing/invalid field).</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePatient(Patient patient)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid patient data."));

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatient), new { id = patient.Id },
                ApiResponse<Patient>.Ok(patient, "Patient created successfully."));
        }

        /// <summary>
        /// Updates an existing patient's details.
        /// </summary>
        /// <param name="id">The patient's ID.</param>
        /// <param name="updated">The new field values to apply.</param>
        /// <response code="200">Patient updated successfully.</response>
        /// <response code="404">No patient exists with this ID.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePatient(int id, Patient updated)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound(ApiResponse<string>.Fail("Patient not found."));

            patient.FirstName = updated.FirstName;
            patient.LastName = updated.LastName;
            patient.Age = updated.Age;
            patient.Gender = updated.Gender;
            patient.PhoneNumber = updated.PhoneNumber;
            patient.Email = updated.Email;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<Patient>.Ok(patient, "Patient updated successfully."));
        }

        /// <summary>
        /// Deactivates a patient (soft-delete — the record is kept, not removed). Admin only.
        /// </summary>
        /// <param name="id">The patient's ID.</param>
        /// <response code="200">Patient deactivated successfully.</response>
        /// <response code="400">Patient is already inactive.</response>
        /// <response code="403">Caller does not have the Admin role.</response>
        /// <response code="404">No patient exists with this ID.</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound(ApiResponse<string>.Fail("Patient not found."));

            if (!patient.IsActive)
                return BadRequest(ApiResponse<string>.Fail("Patient is already inactive."));

            patient.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Patient>.Ok(patient, "Patient deactivated successfully."));
        }

        /// <summary>
        /// Reactivates a previously deactivated patient. Admin only.
        /// </summary>
        /// <param name="id">The patient's ID.</param>
        /// <response code="200">Patient reactivated successfully.</response>
        /// <response code="400">Patient is already active.</response>
        /// <response code="403">Caller does not have the Admin role.</response>
        /// <response code="404">No patient exists with this ID.</response>
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}/reactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reactivate(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound(ApiResponse<string>.Fail("Patient not found."));

            if (patient.IsActive)
                return BadRequest(ApiResponse<string>.Fail("Patient is already active."));

            patient.IsActive = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Patient>.Ok(patient, "Patient reactivated successfully."));
        }
    }
}