using System.ComponentModel.DataAnnotations;

namespace HealthcareCRM.API.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Range(0, 120)]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; } = "";

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}