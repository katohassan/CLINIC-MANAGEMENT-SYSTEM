using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models
{
    public class AppointmentPreference
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PreferredDate { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? PreferredTime { get; set; }

        [StringLength(100)]
        public string? Type { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }
    }
}
