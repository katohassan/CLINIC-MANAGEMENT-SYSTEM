using ClinicAppointmentSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicAppointmentSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null) 
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<ClinicAppointmentSystem.Models.Patient> Patients { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.Doctor> Doctors { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.Staff> Staff { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.EmergencyContact> EmergencyContacts { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.AppointmentPreference> AppointmentPreferences { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.Appointment> Appointments { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.Record> Records { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.ClinicService> ClinicServices { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.Bill> Bills { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.AuditLog> AuditLogs { get; set; } = default!;
    public DbSet<ClinicAppointmentSystem.Models.DoctorAvailability> DoctorAvailabilities { get; set; } = default!;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaveChanges();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        OnBeforeSaveChanges();
        return base.SaveChanges();
    }

    private void OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();
        var userId = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            userId = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        }

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                UserId = userId
            };
            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.AuditType = "Create";
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        auditEntry.ChangedColumns.Add(propertyName);
                        break;

                    case EntityState.Deleted:
                        auditEntry.AuditType = "Delete";
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        auditEntry.ChangedColumns.Add(propertyName);
                        break;

                    case EntityState.Modified:
                        if (property.IsModified && property.OriginalValue?.Equals(property.CurrentValue) != true)
                        {
                            auditEntry.AuditType = "Update";
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            auditEntry.ChangedColumns.Add(propertyName);
                        }
                        break;
                }
            }
        }

        foreach (var auditEntry in auditEntries)
        {
            AuditLogs.Add(auditEntry.ToAuditLog());
        }
    }

    protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure one-to-one between Patient and EmergencyContact where Patient holds the FK
        builder.Entity<Patient>()
            .HasOne(p => p.EmergencyContact)
            .WithOne(ec => ec.Patient)
            .HasForeignKey<Patient>(p => p.EmergencyContactId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.SetNull);

        // Patient preferred doctor (many-to-one)
        builder.Entity<Patient>()
            .HasOne(p => p.PreferredDoctor)
            .WithMany(d => d.AssignedPatients)
            .HasForeignKey(p => p.PreferredDoctorId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.SetNull);

        // AppointmentPreferences relationship
        builder.Entity<AppointmentPreference>()
            .HasOne(ap => ap.Patient)
            .WithMany(p => p.AppointmentPreferences)
            .HasForeignKey(ap => ap.PatientId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
    }
}
