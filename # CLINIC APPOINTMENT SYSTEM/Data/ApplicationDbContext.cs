using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointmentSystem.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<ClinicAppointmentSystem.Models.Patient> Patients { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.Doctor> Doctors { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.Appointment> Appointments { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.Record> Records { get; set; } = default!;
}
