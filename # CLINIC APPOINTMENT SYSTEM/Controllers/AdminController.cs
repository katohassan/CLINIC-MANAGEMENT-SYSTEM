using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicAppointmentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalPatients = await _context.Patients.CountAsync(),
                TotalDoctors = await _context.Doctors.CountAsync(),
                TotalAppointments = await _context.Appointments.CountAsync(),
                PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == "Pending"),
                RecentAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .OrderByDescending(a => a.Date)
                    .Take(5)
                    .ToListAsync()
            };

            return View(stats);
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public System.Collections.Generic.List<ClinicAppointmentSystem.Models.Appointment> RecentAppointments { get; set; } = new();
    }
}
