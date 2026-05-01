using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace ClinicAppointmentSystem.Controllers;

[Authorize(Roles = "Admin")]
public class ClinicServicesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClinicServicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: ClinicServices
    public async Task<IActionResult> Index()
    {
        return View(await _context.ClinicServices.ToListAsync());
    }

    // GET: ClinicServices/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: ClinicServices/Create
    [HttpPost]
    
    public async Task<IActionResult> Create([Bind("Id,Name,Description,Price")] ClinicService clinicService)
    {
        if (ModelState.IsValid)
        {
            _context.Add(clinicService);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(clinicService);
    }

    // GET: ClinicServices/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var clinicService = await _context.ClinicServices.FindAsync(id);
        if (clinicService == null) return NotFound();
        
        return View(clinicService);
    }

    // POST: ClinicServices/Edit/5
    [HttpPost]
    
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price")] ClinicService clinicService)
    {
        if (id != clinicService.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(clinicService);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClinicServiceExists(clinicService.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(clinicService);
    }

    // GET: ClinicServices/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var clinicService = await _context.ClinicServices.FirstOrDefaultAsync(m => m.Id == id);
        if (clinicService == null) return NotFound();

        return View(clinicService);
    }

    // POST: ClinicServices/Delete/5
    [HttpPost, ActionName("Delete")]
    
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var clinicService = await _context.ClinicServices.FindAsync(id);
        if (clinicService != null)
        {
            _context.ClinicServices.Remove(clinicService);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ClinicServiceExists(int id)
    {
        return _context.ClinicServices.Any(e => e.Id == id);
    }
}
