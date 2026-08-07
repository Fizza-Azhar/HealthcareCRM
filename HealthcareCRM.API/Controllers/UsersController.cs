using HealthcareCRM.API.DTOs;
using HealthcareCRM.API.Data;
using HealthcareCRM.API.Helpers;
using HealthcareCRM.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HealthcareCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.Role, u.IsActive })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(users));
        }

        // PUT: api/users/{id}/role
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
        {
            var validRoles = new[] { "Staff", "Admin" };
            if (!validRoles.Contains(dto.Role))
                return BadRequest(ApiResponse<string>.Fail("Invalid role. Must be Staff or Admin."));

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail("User not found."));

            user.Role = dto.Role;
            await _context.SaveChangesAsync();
            await LogAction("RoleChanged", id);

            return Ok(ApiResponse<string>.Ok(user.Role, "Role updated successfully."));
        }

        // PUT: api/users/{id}/toggle-active
        [HttpPut("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail("User not found."));

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            await LogAction(user.IsActive ? "UserActivated" : "UserDeactivated", id);

            return Ok(ApiResponse<bool>.Ok(user.IsActive,
                user.IsActive ? "User activated successfully." : "User deactivated successfully."));
        }

        private async Task LogAction(string action, int targetId)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminId = int.TryParse(adminIdClaim, out var id) ? id : 0;

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = adminId,
                Action = action,
                TargetId = targetId,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}