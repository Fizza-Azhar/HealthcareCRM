
using HealthcareCRM.API.Data;
using HealthcareCRM.API.Models;
using HealthcareCRM.API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/doctors
        // GET: api/doctors?includeInactive=true
        [HttpGet]
        public async Task<IActionResult> GetDoctors([FromQuery] bool includeInactive = false)
        {
            var query = _context.Doctors.AsQueryable();

            if (!includeInactive)
                query = query.Where(d => d.IsActive);

            var doctors = await query.ToListAsync();
            return Ok(ApiResponse<List<Doctor>>.Ok(doctors));
        }

        // GET: api/doctors/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
                return NotFound(ApiResponse<Doctor>.Fail("Doctor not found."));

            return Ok(ApiResponse<Doctor>.Ok(doctor));
        }

        // POST: api/doctors
        [HttpPost]
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

        // PUT: api/doctors/5
        [HttpPut("{id}")]
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

        // PUT: api/doctors/5/deactivate
        [HttpPut("{id}/deactivate")]
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

        // PUT: api/doctors/5/reactivate
        [HttpPut("{id}/reactivate")]
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