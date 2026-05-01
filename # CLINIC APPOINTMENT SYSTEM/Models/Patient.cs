using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public enum Gender
    {
        Unknown = 0,
        Male,
        Female,
        Other
    }

    public class Patient
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        // Personal details
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => string.IsNullOrWhiteSpace(MiddleName) ? $"{FirstName} {LastName}" : $"{FirstName} {MiddleName} {LastName}";

        // Backwards-compatible aliases for existing code
        [NotMapped]
        public string Name { get => FullName; }

        [NotMapped]
        [Display(Name = "DOB")]
        public DateTime DOB { get { return DateOfBirth; } set { DateOfBirth = value; } }

        [NotMapped]
        public string Contact { get { return PrimaryPhone; } set { PrimaryPhone = value; } }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; } = Gender.Unknown;

        [Required]
        [StringLength(100)]
        [Display(Name = "National ID / Passport")]
        public string NationalId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        // Contact
        [Required]
        [Phone]
        [Display(Name = "Primary Phone")]
        public string PrimaryPhone { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Secondary Phone")]
        public string? SecondaryPhone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Emergency contact
        public int? EmergencyContactId { get; set; }
        public EmergencyContact? EmergencyContact { get; set; }

        // Medical details
        [Display(Name = "Allergies")]
        public string? Allergies { get; set; }

        [Display(Name = "Chronic Conditions")]
        public string? ChronicConditions { get; set; }

        [Display(Name = "Past Surgeries")]
        public string? Surgeries { get; set; }

        [Display(Name = "Current Medications")]
        public string? CurrentMedications { get; set; }

        // Insurance
        [Display(Name = "Insurance Provider")]
        public string? InsuranceProvider { get; set; }

        [Display(Name = "Policy Number")]
        public string? PolicyNumber { get; set; }

        // Appointment preferences
        public int? PreferredDoctorId { get; set; }
        public Doctor? PreferredDoctor { get; set; }

        [StringLength(500)]
        [Display(Name = "Reason For Visit")]
        public string? ReasonForVisit { get; set; }

        public ICollection<AppointmentPreference>? AppointmentPreferences { get; set; }

        // Consent
        public bool ConsentDataPrivacy { get; set; } = false;
        public bool ConsentTreatment { get; set; } = false;
        public bool ConsentBilling { get; set; } = false;

        // Navigation
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Record>? Records { get; set; }
    }
}
