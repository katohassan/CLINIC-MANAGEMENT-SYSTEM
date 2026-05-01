using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models
{
    public class Record
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
        public string Notes { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
    }
}
