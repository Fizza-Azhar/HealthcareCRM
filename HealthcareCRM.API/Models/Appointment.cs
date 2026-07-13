using System.ComponentModel.DataAnnotations;

namespace HealthcareCRM.API.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        public string Status { get; set; } = "Pending";

        public string? Notes { get; set; }

        // Navigation Properties
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
    }
}