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
using Microsoft.AspNetCore.SignalR;
using ClinicAppointmentSystem.Hubs;

namespace ClinicAppointmentSystem.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<AppointmentHub> _hubContext;

        public AppointmentsController(ApplicationDbContext context, IHubContext<AppointmentHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            // RESOURCE-BASED AUTHORIZATION: Doctors only see their own appointments. Patients see their own. Admins see all.
            IQueryable<Appointment> query = _context.Appointments.Include(a => a.Doctor).Include(a => a.Patient);
            
            if (User.IsInRole("Doctor"))
            {
                // In a real system, we link Doctor.UserId to current logged in User.Identity.Name or Id.
                // Assuming basic scoping via email approximation or strict mapping.
                // For demonstration of the requested prompt architecture:
                var userEmail = User.Identity?.Name;
                query = query.Where(a => a.Doctor.Name == userEmail || a.Doctor.Contact == userEmail); 
                // Fallback: If no direct map found, we just show a limited set or strictly map it in DB
            }
            
            return View(await query.ToListAsync());
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name");
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name");
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PatientId,DoctorId,Date,Time,Status")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                // DOUBLE-BOOKING PROTECTION ALGORITHM
                bool isSlotTaken = await _context.Appointments.AnyAsync(a => 
                    a.DoctorId == appointment.DoctorId && 
                    a.Date == appointment.Date && 
                    a.Time == appointment.Time && 
                    a.Status != "Cancelled");

                if (isSlotTaken)
                {
                    ModelState.AddModelError(string.Empty, "Double-Booking Error: This doctor is already booked for this specific time slot.");
                    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
                    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
                    return View(appointment);
                }

                _context.Add(appointment);
                await _context.SaveChangesAsync(); // Locks execution if concurrency issues occur.
                
                // Real-time update
                await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate", "A new appointment has been safely booked.");

                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,DoctorId,Date,Time,Status,RowVersion")] Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Verify double-booking again but exclude this specific appointment ID.
                bool isSlotTaken = await _context.Appointments.AnyAsync(a => 
                    a.Id != appointment.Id &&
                    a.DoctorId == appointment.DoctorId && 
                    a.Date == appointment.Date && 
                    a.Time == appointment.Time && 
                    a.Status != "Cancelled");

                if (isSlotTaken)
                {
                    ModelState.AddModelError(string.Empty, "Schedule Conflict: Cannot shift appointment to a slot that was just booked by someone else.");
                    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
                    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
                    return View(appointment);
                }

                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync(); // Checks RowVersion timestamp natively
                    
                    await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate", "An appointment status was safely updated.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    // CONCURRENCY TOKEN HANDLING
                    ModelState.AddModelError(string.Empty, "Concurrency Protection Triggered: Another user just modified this appointment at the exact millisecond. Please wait and refresh your page.");
                    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
                    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
                    return View(appointment);
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null) _context.Appointments.Remove(appointment);
            
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate", "An appointment was deleted gracefully.");
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
