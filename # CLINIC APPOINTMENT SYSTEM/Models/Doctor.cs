using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        // Unique doctor identifier (human-friendly)
        [Required]
        [StringLength(50)]
        public string DoctorId { get; set; } = string.Empty;

        public string? UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => string.IsNullOrWhiteSpace(MiddleName) ? $"{FirstName} {LastName}" : $"{FirstName} {MiddleName} {LastName}";

        // Backwards compatibility
        [NotMapped]
        public string Name { get => FullName; }

        [Required]
        [StringLength(200)]
        public string Specialty { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Qualifications { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [Phone]
        public string? Contact { get; set; }

        [StringLength(500)]
        public string? ConsultationHours { get; set; }

        // Relationships
        public ICollection<Patient>? AssignedPatients { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Record>? Records { get; set; }
        public ICollection<DoctorAvailability>? Availabilities { get; set; }
    }
}
