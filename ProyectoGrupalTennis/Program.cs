using AcademiaTennisBLL.Services;
using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using AcademiaTennisDAL.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to container
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Redirigir al login del proyecto en lugar de /Account/Login (ruta por defecto de Identity)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

// Registrar servicio de InMemoryOfferService
builder.Services.AddScoped<IOfferService, InMemoryOfferService>();

// Registrar servicio de correo
var emailSettings = builder.Configuration
    .GetSection("EmailSettings")
    .Get<EmailSettings>();

builder.Services.AddSingleton(emailSettings);
builder.Services.AddScoped<EmailService>();

//---------- Registrar repositorios y servicios--------------------------
builder.Services.AddScoped<IProfesorRepository, ProfesorRepository>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<ICursoRepository, CursoRepository>();
builder.Services.AddScoped<ICursoService, CursoService>();

//----------------------------------------------------------------------------

var app = builder.Build();

// ── CREAR ROLES AUTOMÁTICAMENTE ──────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    await context.Database.EnsureCreatedAsync();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Administrador", "Profesor", "Usuario" };

    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }
}

// ── CREAR USUARIO ADMINISTRADOR POR DEFECTO ───────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string adminEmail = "admin@tennis.com";
    string adminPassword = "Admin123";

    var adminExistente = await userManager.FindByEmailAsync(adminEmail);
    if (adminExistente == null)
    {
        var admin = new ApplicationUser
        {
            Nombre = "Admin",
            Apellido = "Sistema",
            UserName = adminEmail,
            Email = adminEmail,
            FechaRegistro = DateTime.Now,
            Bloqueado = false
        };

        var resultado = await userManager.CreateAsync(admin, adminPassword);
        if (resultado.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Administrador");
        }
    }
}

// ── PIPELINE ──────────────────────────────────────────────────────────────────

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();