using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicAppointmentSystem.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    // List of allowed major email providers
    private readonly string[] _allowedEmailDomains = { "gmail.com", "yahoo.com", "outlook.com", "hotmail.com", "icloud.com" };

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
                return View(model);
            }

            // Check if email is confirmed (if required)
            if (await _userManager.IsEmailConfirmedAsync(user) == false)
            {
                ModelState.AddModelError(string.Empty, "You must confirm your email before logging in.");
                return View(model);
            }

            if (!user.IsApproved)
            {
                ModelState.AddModelError(string.Empty, "Your account is pending admin approval or has been revoked. You cannot access the system.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Log successful login
                TempData["SuccessMessage"] = "Welcome back!";
                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }
            
            if (result.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var remainingTime = lockoutEnd.Value - DateTimeOffset.UtcNow;
                
                if (remainingTime.TotalMinutes > 0)
                {
                    ModelState.AddModelError(string.Empty, $"Account locked. Try again in {Math.Ceiling(remainingTime.TotalMinutes)} minutes.");
                }
                else
                {
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    ModelState.AddModelError(string.Empty, "Account unlocked. Please try logging in again.");
                }
            }
            else
            {
                var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
                var attemptsRemaining = 5 - failedAttempts;
                
                if (attemptsRemaining > 0)
                {
                    ModelState.AddModelError(string.Empty, $"Invalid password. You have {attemptsRemaining} attempt(s) remaining.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
        }

        return View(model);
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

            var isApproved = model.RoleRequested == "Patient";
            var user = new ApplicationUser 
            { 
                UserName = model.Email, 
                Email = model.Email, 
                FullName = model.Name,
                IsApproved = isApproved,
                RoleRequested = model.RoleRequested,
                PhoneNumber = model.Contact,
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign Requested Role
                await _userManager.AddToRoleAsync(user, model.RoleRequested);

                if (model.RoleRequested == "Patient")
                {
                    // Create Patient Profile
                    // split full name into parts
                    var names = (model.Name ?? string.Empty).Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                    var first = names.Length > 0 ? names[0] : string.Empty;
                    var last = names.Length > 1 ? string.Join(' ', names.Skip(1)) : string.Empty;

                    var patient = new Patient
                    {
                        FirstName = first,
                        LastName = last,
                        DateOfBirth = model.DateOfBirth,
                        PrimaryPhone = model.Contact,
                        Address = model.Address,
                        Allergies = model.Bio,
                        UserId = user.Id
                    };
                    _context.Patients.Add(patient);
                }
                else if (model.RoleRequested == "Doctor")
                {
                    // require qualification/license for doctors
                    if (string.IsNullOrWhiteSpace(model.Qualification) || string.IsNullOrWhiteSpace(model.LicenseNumber))
                    {
                        ModelState.AddModelError(string.Empty, "Doctors must provide qualification and license/registration number for admin verification.");
                        return View(model);
                    }

                    var specialtyText = model.Specialty ?? "General";
                    // Pack professional details into specialty field for admin review (no DB migration required)
                    specialtyText = $"{specialtyText} (Qualification: {model.Qualification}; License: {model.LicenseNumber})";

                    var dnames = (model.Name ?? string.Empty).Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                    var dfirst = dnames.Length > 0 ? dnames[0] : string.Empty;
                    var dlast = dnames.Length > 1 ? string.Join(' ', dnames.Skip(1)) : string.Empty;
                    var doctor = new Doctor
                    {
                        FirstName = dfirst,
                        LastName = dlast,
                        Specialty = specialtyText,
                        Contact = model.Contact,
                        UserId = user.Id
                    };
                    _context.Doctors.Add(doctor);
                }
                else if (model.RoleRequested == "Staff")
                {
                    // Staff also require qualification/license for verification
                    if (string.IsNullOrWhiteSpace(model.Qualification) || string.IsNullOrWhiteSpace(model.LicenseNumber))
                    {
                        ModelState.AddModelError(string.Empty, "Staff members must provide qualification and license/registration number for admin verification.");
                        return View(model);
                    }

                    var specialtyText = $"Staff (Qualification: {model.Qualification}; License: {model.LicenseNumber})";
                    var snames = (model.Name ?? string.Empty).Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                    var sfirst = snames.Length > 0 ? snames[0] : string.Empty;
                    var slast = snames.Length > 1 ? string.Join(' ', snames.Skip(1)) : string.Empty;
                    var staffAsDoctor = new Doctor
                    {
                        FirstName = sfirst,
                        LastName = slast,
                        Specialty = specialtyText,
                        Contact = model.Contact,
                        UserId = user.Id
                    };
                    _context.Doctors.Add(staffAsDoctor);
                }
                
                await _context.SaveChangesAsync();

                if (!isApproved)
                {
                    TempData["SuccessMessage"] = "Registration successful! Your account is pending admin approval.";
                    return RedirectToAction("Login");
                }

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
