using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Cancelled, Completed

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
