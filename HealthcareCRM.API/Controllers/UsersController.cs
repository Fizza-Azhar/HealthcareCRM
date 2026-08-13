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
    /// <summary>
    /// Manages user accounts, roles, and active status. All endpoints require the Admin role.
    /// Every role change and status change is recorded to the audit log automatically.
    /// </summary>
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

        /// <summary>
        /// Returns all users with their role and active status.
        /// </summary>
        /// <response code="200">Returns the list of users.</response>
        /// <response code="403">Caller does not have the Admin role.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.Role, u.IsActive })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(users));
        }

        /// <summary>
        /// Changes a user's role between "Staff" and "Admin".
        /// </summary>
        /// <param name="id">The user's ID.</param>
        /// <param name="dto">The new role. Must be exactly "Staff" or "Admin".</param>
        /// <response code="200">Role updated successfully.</response>
        /// <response code="400">The role value is not "Staff" or "Admin".</response>
        /// <response code="403">Caller does not have the Admin role.</response>
        /// <response code="404">No user exists with this ID.</response>
        [HttpPut("{id}/role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Toggles a user's active status. Deactivated users are blocked from logging in,
        /// even with correct credentials.
        /// </summary>
        /// <param name="id">The user's ID.</param>
        /// <response code="200">Status toggled successfully. Response indicates the new state.</response>
        /// <response code="403">Caller does not have the Admin role.</response>
        /// <response code="404">No user exists with this ID.</response>
        [HttpPut("{id}/toggle-active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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