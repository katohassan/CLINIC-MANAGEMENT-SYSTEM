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
    [Authorize]
    public class RecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Records
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Records.Include(r => r.Doctor).Include(r => r.Patient);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Records/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @record = await _context.Records
                .Include(r => r.Doctor)
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@record == null) return NotFound();

            return View(@record);
        }

        // GET: Records/Create
        [Authorize(Roles = "Doctor,Admin")]
        public IActionResult Create()
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name");
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Create([Bind("Id,PatientId,DoctorId,Notes,Date")] Record @record)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@record);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", @record.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", @record.PatientId);
            return View(@record);
        }

        // GET: Records/Edit/5
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @record = await _context.Records.FindAsync(id);
            if (@record == null) return NotFound();
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", @record.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", @record.PatientId);
            return View(@record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,DoctorId,Notes,Date")] Record @record)
        {
            if (id != @record.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@record);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecordExists(@record.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", @record.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", @record.PatientId);
            return View(@record);
        }

        // GET: Records/Delete/5
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @record = await _context.Records
                .Include(r => r.Doctor)
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@record == null) return NotFound();

            return View(@record);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @record = await _context.Records.FindAsync(id);
            if (@record != null)  _context.Records.Remove(@record);
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecordExists(int id)
        {
            return _context.Records.Any(e => e.Id == id);
        }
    }
}
