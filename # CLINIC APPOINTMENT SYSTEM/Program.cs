using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClinicAppointmentSystem.Data;
using ClinicAppointmentSystem.Hubs;
using ClinicAppointmentSystem.Models;
using ClinicAppointmentSystem.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // ==================== ENHANCED PASSWORD SECURITY ====================
    options.Password.RequireDigit = true;              // Must contain at least one digit
    options.Password.RequireLowercase = true;           // Must contain at least one lowercase
    options.Password.RequireNonAlphanumeric = true;   // Must contain special character
    options.Password.RequireUppercase = true;          // Must contain at least one uppercase
    options.Password.RequiredLength = 8;              // Minimum 8 characters
    options.Password.RequiredUniqueChars = 1;         // At least 1 unique character

    // ==================== LOCKOUT POLICIES (Brute Force Protection) ====================
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);  // Lock for 15 minutes
    options.Lockout.MaxFailedAccessAttempts = 5;       // Lock after 5 failed attempts
    options.Lockout.AllowedForNewUsers = true;         // Apply to new users too

    // ==================== USER SETTINGS ====================
    options.User.RequireUniqueEmail = true;            // Each user must have unique email
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";  // Allowed chars
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender, EmailSender>();

// ==================== COOKIE & SESSION SECURITY ====================
builder.Services.ConfigureApplicationCookie(options =>
{
    // HttpOnly: Prevents JavaScript access to cookies (XSS protection)
    options.Cookie.HttpOnly = true;

    // SecurePolicy.Always: Cookie only sent over HTTPS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // SameSite.Strict: Prevents CSRF attacks
    options.Cookie.SameSite = SameSiteMode.Strict;

    // Session timeout
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;  // Reset timeout on each request

    // Custom login paths
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ==================== ADDITIONAL SECURITY HEADERS ====================
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.FormFieldName = "__RequestVerificationToken";
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

// ==================== SESSION CONFIGURATION ====================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

// ==================== DATABASE INITIALIZATION ====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await DbInitializer.Initialize(services, userManager, roleManager);
}

// ==================== HTTP REQUEST PIPELINE ====================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // HTTP Strict Transport Security
}

app.UseHttpsRedirection();  // Force HTTPS
app.UseRouting();

app.UseSession();  // Enable sessions
app.UseAuthentication();
app.UseAuthorization();

// Redirect Identity area pages to custom MVC AccountController views so scaffolded Identity pages are not shown
app.MapGet("/Identity/Account/Register", () => Results.Redirect("/Account/Register"));
app.MapGet("/Identity/Account/Login", () => Results.Redirect("/Account/Login"));

// ==================== SECURITY HEADERS MIDDLEWARE ====================
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.MapHub<AppointmentHub>("/appointmentHub");

app.Run();

