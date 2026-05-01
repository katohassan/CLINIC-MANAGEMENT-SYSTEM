using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace ClinicAppointmentSystem.Controllers
{
    [Authorize(Roles = "Admin,Doctor")]
    public class DoctorAvailabilitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorAvailabilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DoctorAvailabilities
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.DoctorAvailabilities.Include(d => d.Doctor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: DoctorAvailabilities/Create
        public IActionResult Create()
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DoctorId,DayOfWeek,StartTime,EndTime")] DoctorAvailability doctorAvailability)
        {
            if (ModelState.IsValid)
            {
                _context.Add(doctorAvailability);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", doctorAvailability.DoctorId);
            return View(doctorAvailability);
        }

        // GET: DoctorAvailabilities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var doctorAvailability = await _context.DoctorAvailabilities.FindAsync(id);
            if (doctorAvailability == null) return NotFound();
            
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", doctorAvailability.DoctorId);
            return View(doctorAvailability);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DoctorId,DayOfWeek,StartTime,EndTime")] DoctorAvailability doctorAvailability)
        {
            if (id != doctorAvailability.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctorAvailability);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorAvailabilityExists(doctorAvailability.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", doctorAvailability.DoctorId);
            return View(doctorAvailability);
        }

        // GET: DoctorAvailabilities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var doctorAvailability = await _context.DoctorAvailabilities
                .Include(d => d.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (doctorAvailability == null) return NotFound();

            return View(doctorAvailability);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctorAvailability = await _context.DoctorAvailabilities.FindAsync(id);
            if (doctorAvailability != null) _context.DoctorAvailabilities.Remove(doctorAvailability);
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorAvailabilityExists(int id)
        {
            return _context.DoctorAvailabilities.Any(e => e.Id == id);
        }
    }
}
