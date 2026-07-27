using AcademiaTennisBLL.Services;
using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using AcademiaTennisDAL.Repositories;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to container
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection"))));

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
builder.Services.AddScoped<ProyectoGrupalTennis.Services.ChatbotService>(); // Módulo 6 - Asistente Virtual
//----------------------------------------------------------------------------
//REGISTRAR SERVICIO DE CALENDAR
builder.Services.AddScoped<GoogleCalendarService>();
builder.Services.AddSession(); // para guardar returnUrl

// HANGFIRE
builder.Services.AddHangfire(config =>
{
    config.UseStorage(new MySqlStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlStorageOptions
        {
            TablesPrefix = "Hangfire"
        }));
});

builder.Services.AddHangfireServer();
var app = builder.Build();

// ── CREAR ROLES AUTOMÁTICAMENTE ──────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    //await context.Database.EnsureCreatedAsync();

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

    if (await userManager.FindByEmailAsync(adminEmail) == null)
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

        var resultado = await userManager.CreateAsync(admin, "Admin123");

        if (resultado.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Administrador");
        }
    }
    string profesorEmail = "profesor@tennis.com";

    if (await userManager.FindByEmailAsync(profesorEmail) == null)
    {
        var profesor = new ApplicationUser
        {
            Nombre = "Profesor",
            Apellido = "Sistema",
            UserName = profesorEmail,
            Email = profesorEmail,
            FechaRegistro = DateTime.Now,
            Bloqueado = false
        };

        var resultado = await userManager.CreateAsync(profesor, "Profesor123");

        if (resultado.Succeeded)
        {
            await userManager.AddToRoleAsync(profesor, "Profesor");
        }
    }
    string alumnoEmail = "alumno@tennis.com";

    if (await userManager.FindByEmailAsync(alumnoEmail) == null)
    {
        var alumno = new ApplicationUser
        {
            Nombre = "Alumno",
            Apellido = "Sistema",
            UserName = alumnoEmail,
            Email = alumnoEmail,
            FechaRegistro = DateTime.Now,
            Bloqueado = false
        };

        var resultado = await userManager.CreateAsync(alumno, "Alumno123");

        if (resultado.Succeeded)
        {
            await userManager.AddToRoleAsync(alumno, "Usuario");
        }
    }
}

// ── SEED DE PREGUNTAS FRECUENTES DEL ASISTENTE VIRTUAL (Módulo 6) ────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!await context.PreguntasFrecuentes.AnyAsync())
    {
        context.PreguntasFrecuentes.AddRange(
            new AcademiaTennisDAL.Entities.PreguntaFrecuente
            {
                Pregunta = "¿Cuál es el horario de la academia?",
                Categoria = "Horarios",
                Respuesta = "Nuestro horario es de 6:00 a.m. a 10:00 p.m., todos los días. " +
                             "Puedes acomodar tus lecciones dentro de ese horario según tu disponibilidad."
            },
            new AcademiaTennisDAL.Entities.PreguntaFrecuente
            {
                Pregunta = "¿Dónde están ubicados?",
                Categoria = "Ubicación",
                Respuesta = "Estamos ubicados en el OTA Center, San Antonio de Desamparados, contiguo a la " +
                             "Escuela Panamá (San José, CR-SJ, 10305). Puedes ver el mapa en la parte de abajo " +
                             "de esta página o visitar http://www.tenismmp.com/"
            },
            new AcademiaTennisDAL.Entities.PreguntaFrecuente
            {
                Pregunta = "¿Cuánto cuestan las clases?",
                Categoria = "Precios",
                Respuesta = "Los precios varían según el paquete (2, 4, 6 o 10 lecciones de 55 minutos) y si la " +
                             "clase es individual, en pareja o grupal (en pareja/grupo el precio es por persona). " +
                             "Entre más grande el paquete, mayor el descuento. Puedes ver el catálogo completo y " +
                             "los precios actualizados en la sección de \"Clases y paquetes\" antes de solicitar tu clase."
            },
            new AcademiaTennisDAL.Entities.PreguntaFrecuente
            {
                Pregunta = "¿Qué tipos de clases ofrecen?",
                Categoria = "General",
                Respuesta = "Ofrecemos clases para todos los niveles (principiantes, intermedios y avanzados), " +
                             "de forma individual, en pareja o en grupo, con o sin matrícula. También tenemos " +
                             "clases específicas de fin de semana. Cada clase sigue una ficha técnica para " +
                             "asegurar tu progreso paso a paso."
            },
            new AcademiaTennisDAL.Entities.PreguntaFrecuente
            {
                Pregunta = "¿Necesito llevar mi propio equipo?",
                Categoria = "General",
                Respuesta = "No es obligatorio. Si no tienes raqueta u otro equipo, nosotros te lo prestamos sin costo adicional."
            },
            new AcademiaTennisDAL.Entities.PreguntaFrecuente
            {
                Pregunta = "¿Qué instalaciones tiene el OTA Center?",
                Categoria = "General",
                Respuesta = "El OTA Center cuenta con 5 canchas de tenis (2 bajo techo y 3 al aire libre), " +
                             "3 canchas de pádel, 1 cancha de pickleball, 3 mesas de pool, además de seguridad y parqueo."
            },
            new AcademiaTennisDAL.Entities.PreguntaFrecuente
            {
                Pregunta = "¿Los profesores están certificados?",
                Categoria = "General",
                Respuesta = "Sí, todos nuestros profesores están certificados por la I.T.F. (International Tennis " +
                             "Federation) y la P.T.R. (Professional Tennis Registry)."
            },
            new AcademiaTennisDAL.Entities.PreguntaFrecuente
            {
                Pregunta = "¿Cómo solicito una clase?",
                Categoria = "General",
                Respuesta = "Puedes elegir el paquete que prefieras desde la sección \"Clases y paquetes\", indicar " +
                             "tu disponibilidad de horario y enviar la solicitud. La academia se pondrá en contacto " +
                             "contigo para confirmar el horario definitivo."
            }
        );

        await context.SaveChangesAsync();
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

app.UseSession();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();
app.UseHangfireDashboard("/hangfire");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();