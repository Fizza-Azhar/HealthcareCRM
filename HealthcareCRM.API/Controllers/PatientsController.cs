using HealthcareCRM.API.Data;
using HealthcareCRM.API.Helpers;
using HealthcareCRM.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareCRM.API.Controllers
{
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

        // GET: api/patients
[HttpGet]
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

        // GET: api/patients/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return NotFound(ApiResponse<Patient>.Fail("Patient not found."));

            return Ok(ApiResponse<Patient>.Ok(patient));
        }

        // POST: api/patients
        [HttpPost]
        public async Task<IActionResult> CreatePatient(Patient patient)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid patient data."));

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatient), new { id = patient.Id },
                ApiResponse<Patient>.Ok(patient, "Patient created successfully."));
        }

        // PUT: api/patients/1
        [HttpPut("{id}")]
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

        // PUT: api/patients/1/deactivate
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}/deactivate")]
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

        // PUT: api/patients/1/reactivate
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}/reactivate")]
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