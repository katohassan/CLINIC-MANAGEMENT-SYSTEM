using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public class Staff
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string StaffId { get; set; } = string.Empty;

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

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty; // Receptionist, Nurse, LabTech, Admin

        [StringLength(100)]
        public string? Department { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EmploymentDate { get; set; }

        [StringLength(200)]
        public string? ShiftSchedule { get; set; }

        [StringLength(200)]
        public string? AccessLevel { get; set; }

        public string? UserId { get; set; }
    }
}
