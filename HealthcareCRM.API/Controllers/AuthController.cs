using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthcareCRM.API.Data;
using HealthcareCRM.API.Models;
using HealthcareCRM.API.Helpers;

namespace HealthcareCRM.API.Controllers
{
    /// <summary>
    /// Handles user registration and login. These endpoints do not require an existing token —
    /// they are the entry point for obtaining one.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthController(AppDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        /// <summary>
        /// Registers a new user account. New accounts always start with the "Staff" role;
        /// an existing Admin must promote them afterward via the Users endpoints.
        /// </summary>
        /// <param name="request">First name, last name, email, and password. All fields are required; password must be at least 6 characters.</param>
        /// <response code="200">Registration successful.</response>
        /// <response code="400">Validation failed (missing/invalid field), or an account with this email already exists.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid registration data." });

            var existing = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (existing)
                return BadRequest(new { message = "An account with this email already exists." });

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful" });
        }

        /// <summary>
        /// Logs in with email and password and returns a signed JWT token.
        /// Use this token in the "Authorize" button above (or as an
        /// <c>Authorization: Bearer {token}</c> header) to call any protected endpoint.
        /// </summary>
        /// <param name="request">Email and password of an existing, active account.</param>
        /// <response code="200">Login successful. Response includes the JWT token.</response>
        /// <response code="400">Validation failed (missing email/password).</response>
        /// <response code="401">Invalid email or password, or the account has been deactivated by an Admin.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid login data." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            if (!user.IsActive)
                return Unauthorized(new { message = "This account has been deactivated. Contact an administrator." });

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid email or password." });

            var token = _jwtHelper.GenerateToken(user);

            return Ok(new
            {
                message = "Login successful",
                token = token
            });
        }
    }
}