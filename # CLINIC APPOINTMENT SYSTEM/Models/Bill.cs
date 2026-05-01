using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models;

public class Bill
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Appointment")]
    public int AppointmentId { get; set; }

    [ForeignKey("AppointmentId")]
    public Appointment? Appointment { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Total Amount")]
    public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // e.g., Pending, Paid, Overdue

    [Display(Name = "Issued Date")]
    public DateTime IssuedDate { get; set; } = DateTime.Now;

    [Display(Name = "Paid Date")]
    public DateTime? PaidDate { get; set; }

    public string? Notes { get; set; }
}
