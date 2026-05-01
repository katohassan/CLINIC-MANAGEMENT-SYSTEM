using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Specialty { get; set; } = string.Empty;

        [Required]
        public string Contact { get; set; } = string.Empty;

        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Record>? Records { get; set; }
    }
}
