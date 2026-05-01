using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicAppointmentSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AuditLogsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuditLogsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: AuditLogs
    public async Task<IActionResult> Index(string searchString, string typeFilter)
    {
        var logsQuery = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            logsQuery = logsQuery.Where(l => l.TableName.Contains(searchString) || 
                                             l.UserId.Contains(searchString) ||
                                             l.PrimaryKey.Contains(searchString));
        }

        if (!string.IsNullOrEmpty(typeFilter))
        {
            logsQuery = logsQuery.Where(l => l.Type == typeFilter);
        }

        var logs = await logsQuery.OrderByDescending(l => l.DateTime).Take(100).ToListAsync();
        
        ViewData["CurrentFilter"] = searchString;
        ViewData["TypeFilter"] = typeFilter;
        
        return View(logs);
    }

    // GET: AuditLogs/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var auditLog = await _context.AuditLogs
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (auditLog == null)
        {
            return NotFound();
        }

        return View(auditLog);
    }
}
