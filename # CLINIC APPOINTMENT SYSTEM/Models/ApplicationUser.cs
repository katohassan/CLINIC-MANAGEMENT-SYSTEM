using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public bool IsApproved { get; set; } = false; // Admin approval required
    
    public string RoleRequested { get; set; } = "Patient"; // "Patient", "Doctor", "Staff"
}
