using System.ComponentModel.DataAnnotations;

namespace HealthcareCRM.API.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        public string Role { get; set; } = "Staff"; // e.g. Staff, Admin

        public bool IsActive { get; set; } = true;
    }
}