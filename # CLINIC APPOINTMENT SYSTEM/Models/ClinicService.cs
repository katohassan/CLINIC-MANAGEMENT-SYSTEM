using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicAppointmentSystem.Models;

public class ClinicService
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Service Name is required.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Price ($)")]
    public decimal Price { get; set; }
}
