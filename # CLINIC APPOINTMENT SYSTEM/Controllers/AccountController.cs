using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicAppointmentSystem.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ApplicationDbContext _context;

    // List of allowed major email providers
    private readonly string[] _allowedEmailDomains = { "gmail.com", "yahoo.com", "outlook.com", "hotmail.com", "icloud.com" };

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Validate Known Email Channel
            var emailDomain = model.Email.Split('@').LastOrDefault()?.ToLower();
            if (emailDomain == null || !_allowedEmailDomains.Contains(emailDomain))
            {
                ModelState.AddModelError("Email", "Please use a valid email address from a known provider (e.g., Gmail, Yahoo, Outlook, iCloud).");
                return View(model);
            }

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign Patient Role
                await _userManager.AddToRoleAsync(user, "Patient");

                // Create Patient Profile
                var patient = new Patient
                {
                    Name = model.Name,
                    DOB = model.DateOfBirth,
                    Contact = model.Contact,
                    Address = model.Address,
                    MedicalHistory = model.Bio,
                    UserId = user.Id
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                // Automatically sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }
}
