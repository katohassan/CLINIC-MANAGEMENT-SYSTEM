using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models
{
    public class EmergencyContact
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Relationship { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        // Link back to patient (optional)
        public Patient? Patient { get; set; }
    }
}
