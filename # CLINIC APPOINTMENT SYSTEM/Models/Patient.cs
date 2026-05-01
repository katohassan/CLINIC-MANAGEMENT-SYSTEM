using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models
{
    public class Patient
    {
        public int Id { get; set; }
        
        public string? UserId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Required]
        public string Contact { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? MedicalHistory { get; set; }

        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Record>? Records { get; set; }
    }
}
