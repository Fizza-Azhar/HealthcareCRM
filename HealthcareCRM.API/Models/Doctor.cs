using System.ComponentModel.DataAnnotations;

namespace HealthcareCRM.API.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        public string Specialization { get; set; } = "";

        [Required]
        [Phone]
        public string Phone { get; set; } = "";

        public bool IsActive { get; set; } = true;
    }
}