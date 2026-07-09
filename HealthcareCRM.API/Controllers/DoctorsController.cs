using HealthcareCRM.API.Data;
using HealthcareCRM.API.Models;
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
        // Returns only active doctors, per checklist: "list all active doctors"
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Where(d => d.IsActive)
                .ToListAsync();

            return doctors;
        }
    }
}