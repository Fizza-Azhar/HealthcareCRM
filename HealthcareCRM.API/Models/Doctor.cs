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

        [Required]
        public string ScheduleDays { get; set; } = ""; // e.g. "Mon,Wed,Fri"

        public bool IsActive { get; set; } = true;
    }
}