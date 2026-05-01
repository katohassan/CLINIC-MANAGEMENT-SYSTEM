using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicAppointmentSystem.Models;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    // Custom validation can be added via attributes or handled in controller
    public string Email { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Recovery Email (Optional)")]
    public string? RecoveryEmail { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // Patient Information
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [Phone]
    public string Contact { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [DataType(DataType.MultilineText)]
    [Display(Name = "Bio / Medical History")]
    public string? Bio { get; set; }

    [Required]
    [Display(Name = "Register As")]
    public string RoleRequested { get; set; } = "Patient"; // Options: Patient, Doctor, Staff

    [Display(Name = "Medical Specialty")]
    public string? Specialty { get; set; } // Required if RoleRequested is "Doctor"

    [Display(Name = "Qualification / Title")]
    public string? Qualification { get; set; }

    [Display(Name = "License / Registration Number")]
    public string? LicenseNumber { get; set; }
}
