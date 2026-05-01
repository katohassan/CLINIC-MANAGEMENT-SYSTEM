using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;

namespace ClinicAppointmentSystem.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;

    // List of allowed major email providers
    private readonly string[] _allowedEmailDomains = { "gmail.com", "yahoo.com", "outlook.com", "hotmail.com", "icloud.com" };

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _emailSender = emailSender;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    
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
                RecoveryEmail = model.RecoveryEmail,
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
                    TempData["SuccessMessage"] = "Registration successful! Your account is pending admin approval. Please check your email to verify your account.";
                    return RedirectToAction("Login");
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                if (callbackUrl != null)
                {
                    await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                }

                TempData["SuccessMessage"] = "Registration successful! Please check your email to verify your account before logging in.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);
        TempData["SuccessMessage"] = result.Succeeded ? "Thank you for confirming your email. You can now log in." : "Error confirming your email.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                TempData["SuccessMessage"] = "If an account with this email exists and is confirmed, a password reset link has been sent.";
                return RedirectToAction("Login");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { code },
                protocol: Request.Scheme);

            if (callbackUrl != null)
            {
                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            }

            TempData["SuccessMessage"] = "If an account with this email exists and is confirmed, a password reset link has been sent.";
            return RedirectToAction("Login");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPassword(string? code = null)
    {
        if (code == null)
        {
            return BadRequest("A code must be supplied for password reset.");
        }
        else
        {
            var model = new ResetPasswordViewModel
            {
                Code = code
            };
            return View(model);
        }
    }

    [HttpPost]
    
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            TempData["SuccessMessage"] = "Your password has been reset successfully. Please log in.";
            return RedirectToAction("Login");
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Your password has been reset successfully. Please log in.";
            return RedirectToAction("Login");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View(model);
    }
}
