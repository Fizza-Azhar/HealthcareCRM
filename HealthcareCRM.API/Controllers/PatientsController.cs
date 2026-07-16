using HealthcareCRM.API.Data;
using HealthcareCRM.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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
// GET: api/patients
// GET: api/patients
[HttpGet]
public async Task<ActionResult<IEnumerable<Patient>>> GetPatients(
    string? search,
    int page = 1,
    int pageSize = 20)
{
    var query = _context.Patients.AsQueryable();

    // Search
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(p =>
            p.FirstName.Contains(search) ||
            p.LastName.Contains(search) ||
            p.PhoneNumber.Contains(search));
    }

    // Pagination
    query = query
        .Skip((page - 1) * pageSize)
        .Take(pageSize);

    return await query.ToListAsync();
}
// GET: api/patients/1
[HttpGet("{id}")]
public async Task<ActionResult<Patient>> GetPatient(int id)
{
    var patient = await _context.Patients.FindAsync(id);

    if (patient == null)
    {
        return NotFound();
    }

    return patient;
}
// POST: api/patients
[HttpPost]
public async Task<ActionResult<Patient>> CreatePatient(Patient patient)
{
    _context.Patients.Add(patient);

    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
}
// PUT: api/patients/1
[HttpPut("{id}")]
public async Task<IActionResult> UpdatePatient(int id, Patient patient)
{
    if (id != patient.Id)
    {
        return BadRequest();
    }

    _context.Entry(patient).State = EntityState.Modified;

    await _context.SaveChangesAsync();

    return NoContent();
}
// DELETE: api/patients/1
[HttpDelete("{id}")]
public async Task<IActionResult> DeletePatient(int id)
{
    var patient = await _context.Patients.FindAsync(id);

    if (patient == null)
    {
        return NotFound();
    }

    _context.Patients.Remove(patient);

    await _context.SaveChangesAsync();

    return NoContent();
}
    }
}