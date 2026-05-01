using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace ClinicAppointmentSystem.Models;

public class AuditEntry
{
    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
    }

    public EntityEntry Entry { get; }
    public string? UserId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, object?> KeyValues { get; } = new Dictionary<string, object?>();
    public Dictionary<string, object?> OldValues { get; } = new Dictionary<string, object?>();
    public Dictionary<string, object?> NewValues { get; } = new Dictionary<string, object?>();
    public string AuditType { get; set; } = string.Empty;
    public List<string> ChangedColumns { get; } = new List<string>();

    public AuditLog ToAuditLog()
    {
        var audit = new AuditLog
        {
            UserId = UserId,
            Type = AuditType,
            TableName = TableName,
            DateTime = DateTime.UtcNow,
            PrimaryKey = JsonSerializer.Serialize(KeyValues),
            OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
            NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues),
            AffectedColumns = ChangedColumns.Count == 0 ? null : JsonSerializer.Serialize(ChangedColumns)
        };
        return audit;
    }
}
