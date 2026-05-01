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
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ClinicAppointmentSystem.Models.ApplicationUser> _userManager;

        public PatientsController(ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<ClinicAppointmentSystem.Models.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Doctor") || User.IsInRole("Staff"))
            {
                return View(await _context.Patients.Include(p => p.PreferredDoctor).ToListAsync());
            }

            // Patients see only their own record
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Forbid();

            var patient = await _context.Patients.Include(p => p.PreferredDoctor).FirstOrDefaultAsync(p => p.UserId == currentUser.Id);
            if (patient == null)
            {
                return View(new List<Patient>()); // empty
            }
            return View(new List<Patient> { patient });
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var patient = await _context.Patients.Include(p => p.EmergencyContact).Include(p => p.AppointmentPreferences).Include(p => p.PreferredDoctor).FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        // GET: Patients/Create
        [Authorize(Roles = "Staff")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Create(Patient patient)
        {
            if (ModelState.IsValid)
            {
                // if EmergencyContact provided, attach
                if (patient.EmergencyContact != null)
                {
                    _context.EmergencyContacts.Add(patient.EmergencyContact);
                }
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Doctors = await _context.Doctors.ToListAsync();
            return View(patient);
        }

        // GET: Patients/Edit/5
        [Authorize(Roles = "Admin,Staff,Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            ViewBag.Doctors = await _context.Doctors.ToListAsync();
            return View(patient);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        
        [Authorize(Roles = "Admin,Staff,Doctor")]
        public async Task<IActionResult> Edit(int id, Patient patient)
        {
            if (id != patient.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Doctors = await _context.Doctors.ToListAsync();
            return View(patient);
        }

        // GET: Patients/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients.FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null) _context.Patients.Remove(patient);
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.Id == id);
        }
    }
}
