using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicAppointmentSystem.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicAppointmentSystem.ViewComponents
{
    public class RecentActivityViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public RecentActivityViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var recentRecords = await _context.Records
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .OrderByDescending(r => r.Date)
                .Take(5)
                .ToListAsync();

            return View(recentRecords);
        }
    }
}
