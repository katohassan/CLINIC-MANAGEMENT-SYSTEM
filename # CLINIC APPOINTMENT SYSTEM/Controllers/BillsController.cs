using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicAppointmentSystem.Controllers;

[Authorize(Roles = "Admin,Receptionist")]
public class BillsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BillsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Bills
    public async Task<IActionResult> Index()
    {
        var bills = _context.Bills
            .Include(b => b.Appointment)
                .ThenInclude(a => a.Patient)
            .OrderByDescending(b => b.IssuedDate);
        return View(await bills.ToListAsync());
    }

    // GET: Bills/Create?appointmentId=5
    public async Task<IActionResult> Create(int? appointmentId)
    {
        if (appointmentId == null)
        {
            // If no appointment specified, show selection or handle error
            ViewData["AppointmentId"] = new SelectList(_context.Appointments
                .Include(a => a.Patient)
                .Select(a => new { Id = a.Id, Display = a.Patient.Name + " - " + a.Date.ToShortDateString() }), 
                "Id", "Display");
        }
        else
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
            
            if (appointment == null) return NotFound();
            
            ViewData["SelectedAppointment"] = appointment;
            ViewData["AppointmentId"] = appointment.Id;
        }

        return View();
    }

    // POST: Bills/Create
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("AppointmentId,TotalAmount,Notes,Status")] Bill bill)
    {
        if (ModelState.IsValid)
        {
            bill.IssuedDate = DateTime.Now;
            if (bill.Status == "Paid")
            {
                bill.PaidDate = DateTime.Now;
            }
            
            _context.Add(bill);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        ViewData["AppointmentId"] = new SelectList(_context.Appointments.Include(a => a.Patient), "Id", "Patient.Name", bill.AppointmentId);
        return View(bill);
    }

    // GET: Bills/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var bill = await _context.Bills
            .Include(b => b.Appointment)
                .ThenInclude(a => a.Patient)
            .Include(b => b.Appointment)
                .ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (bill == null) return NotFound();

        return View(bill);
    }

    // POST: Bills/MarkAsPaid/5
    [HttpPost]
    
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var bill = await _context.Bills.FindAsync(id);
        if (bill == null) return NotFound();

        bill.Status = "Paid";
        bill.PaidDate = DateTime.Now;
        
        _context.Update(bill);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }
}
