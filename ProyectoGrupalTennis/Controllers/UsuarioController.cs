using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Helpers;
using ProyectoGrupalTennis.Models;
using ProyectoGrupalTennis.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProyectoGrupalTennis.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly GoogleCalendarService _calendarService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuarioController(
            AppDbContext context,
            EmailService emailService,
            IConfiguration configuration,
            GoogleCalendarService calendarService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _calendarService = calendarService;
            _userManager = userManager;
        }


        // GET: /Usuario/MisCursos
        public async Task<IActionResult> MisCursos(
    string? buscar,
    string? nivel)
        {
            var query = _context.Cursos
                .Include(c => c.Horarios)
                .Include(c => c.Profesor)
                .Where(c => c.Activo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(c =>
                    c.Nombre.Contains(buscar));
            }

            if (!string.IsNullOrWhiteSpace(nivel))
            {
                query = query.Where(c =>
                    c.Nivel == nivel);
            }

            var cursos = await query
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            DateTime ahora = DateTime.Now;

            // Oculta cursos cuyo último horario ya terminó.
            cursos = cursos
                .Where(c =>
                    c.Horarios == null ||
                    !c.Horarios.Any() ||
                    c.Horarios.Any(h =>
                        h.Fecha.Date.Add(h.HoraFin) > ahora))
                .ToList();

            var viewModel = new UsuarioCursosViewModel
            {
                FiltroBuscar = buscar,
                FiltroNivel = nivel,

                Cursos = cursos.Select(c =>
                {
                    DateTime? primerInicio =
                        c.Horarios != null &&
                        c.Horarios.Any()
                            ? c.Horarios
                                .Select(h =>
                                    h.Fecha.Date.Add(h.HoraInicio))
                                .Min()
                            : null;

                    DateTime? fechaLimite =
                        primerInicio?.AddHours(-3);

                    bool matriculaCerrada =
                        fechaLimite.HasValue &&
                        ahora >= fechaLimite.Value;

                    return new CursoUsuarioItemViewModel
                    {
                        IdCurso = c.IdCurso,
                        Nombre = c.Nombre,
                        Descripcion =
                            c.Descripcion ?? string.Empty,
                        Nivel = c.Nivel,
                        CuposDisponibles =
                            c.CuposDisponibles,
                        Precio = c.Precio,

                        NombreProfesor =
                            c.Profesor != null
                                ? $"{c.Profesor.Nombre} {c.Profesor.Apellidos}"
                                : "Sin asignar",

                        Horarios =
                            c.Horarios != null
                                ? c.Horarios
                                    .OrderBy(h => h.Fecha)
                                    .ThenBy(h => h.HoraInicio)
                                    .Select(h =>
                                        $"{h.Fecha:dd/MM/yyyy} - " +
                                        $"{h.DiaSemana} " +
                                        $"{h.HoraInicio:hh\\:mm} - " +
                                        $"{h.HoraFin:hh\\:mm}")
                                    .ToList()
                                : new List<string>(),

                        MatriculaCerrada =
                            matriculaCerrada,

                        FechaLimiteMatricula =
                            fechaLimite?.ToString(
                                "dd/MM/yyyy HH:mm")
                    };
                }).ToList()
            };

            return View(
                "~/Views/Perfiles/UsuarioCursos.cshtml",
                viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmarPagoMatricula(int idCurso)
        {
            string? idAlumno = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(idAlumno))
            {
                return Challenge();
            }

            var curso = await _context.Cursos
                .Include(c => c.Horarios)
                .FirstOrDefaultAsync(c =>
                    c.IdCurso == idCurso &&
                    c.Activo);

            if (curso == null)
            {
                TempData["Error"] = "El curso no existe.";

                return RedirectToAction(nameof(MisCursos));
            }

            if (MatriculaCerrada(curso))
            {
                TempData["Error"] =
                    "La matrícula para este curso ya está cerrada porque faltan menos de 3 horas para iniciar.";

                return RedirectToAction(nameof(MisCursos));
            }

            var ubicacion = await _context.UbicacionesAlumno
                .Include(u => u.Zona)
                .FirstOrDefaultAsync(u =>
                    u.IdAlumno == idAlumno &&
                    u.EsPrincipal);

            var model = new ConfirmarPagoViewModel
            {
                IdCurso = curso.IdCurso,
                Concepto = curso.Nombre,

                Monto = curso.Precio,
                MontoTotal = curso.Precio,

                TieneUbicacion = ubicacion != null,

                IdUbicacionAlumno =
                    ubicacion?.IdUbicacion,

                DireccionCompleta =
                    ubicacion?.DireccionCompleta,

                NombreZona =
                    ubicacion?.Zona?.Nombre
            };

            if (ubicacion?.Zona != null &&
                ubicacion.Zona.LatitudCentro.HasValue &&
                ubicacion.Zona.LongitudCentro.HasValue)
            {
                double distancia = CalcularDistanciaKm(
                    (double)ubicacion.Latitud,
                    (double)ubicacion.Longitud,
                    (double)ubicacion.Zona.LatitudCentro.Value,
                    (double)ubicacion.Zona.LongitudCentro.Value);

                decimal distanciaDecimal =
                    Convert.ToDecimal(distancia);

                decimal costoFijo =
                    ubicacion.Zona.CostoAdicional;

                decimal tarifaKm =
                    ubicacion.Zona.TarifaPorKm ?? 0;

                decimal costoPorDistancia =
                    distanciaDecimal * tarifaKm;

                decimal costoDesplazamiento =
                    costoFijo + costoPorDistancia;

                model.DistanciaKm =
                    Math.Round(distanciaDecimal, 2);

                model.CostoFijoZona =
                    costoFijo;

                model.TarifaPorKm =
                    tarifaKm;

                model.CostoPorDistancia =
                    Math.Round(costoPorDistancia, 2);

                model.CostoDesplazamiento =
                    Math.Round(costoDesplazamiento, 2);
            }

            return View(
                "~/Views/Pagos/ConfirmarPagoMatricula.cshtml",
                model);
        }

        // POST: /Usuario/MatricularCurso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MatricularCurso(
            ConfirmarPagoViewModel modelo)
        {
            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["Error"] =
                    "Debe iniciar sesión para matricular un curso.";

                return RedirectToAction("Login", "Auth");
            }

            var curso = await _context.Cursos
                .Include(c => c.Horarios)
                .FirstOrDefaultAsync(c =>
                    c.IdCurso == modelo.IdCurso &&
                    c.Activo);

            if (curso == null)
            {
                TempData["Error"] =
                    "El curso seleccionado no existe o no está activo.";

                return RedirectToAction(nameof(MisCursos));
            }

            if (curso.CuposDisponibles <= 0)
            {
                TempData["Error"] =
                    "No hay cupos disponibles para este curso.";

                return RedirectToAction(nameof(MisCursos));
            }

            bool yaMatriculado = await _context.Matriculas
                .AnyAsync(m =>
                    m.IdAlumno == userId &&
                    m.IdCurso == modelo.IdCurso &&
                    m.Estado == "Activa");

            if (yaMatriculado)
            {
                TempData["Error"] =
                    "Ya estás matriculado en este curso.";

                return RedirectToAction(nameof(MisCursos));
            }

            decimal distanciaKm = 0;
            decimal costoDesplazamiento = 0;
            int? idUbicacionAlumno = null;

            /*
             * No confiamos en los precios enviados desde la vista.
             * El servidor vuelve a consultar y calcular todo.
             */
            if (modelo.EsADomicilio)
            {
                var ubicacion = await _context.UbicacionesAlumno
                    .Include(u => u.Zona)
                    .FirstOrDefaultAsync(u =>
                        u.IdAlumno == userId &&
                        u.EsPrincipal);

                if (ubicacion == null)
                {
                    TempData["Error"] =
                        "Debe registrar una ubicación antes de solicitar la modalidad a domicilio.";

                    return RedirectToAction(
                        "MiUbicacion",
                        "Geolocalizacion");
                }

                if (ubicacion.Zona == null ||
                    !ubicacion.Zona.Activa ||
                    !ubicacion.Zona.LatitudCentro.HasValue ||
                    !ubicacion.Zona.LongitudCentro.HasValue)
                {
                    TempData["Error"] =
                        "La ubicación registrada no tiene una zona de cobertura válida.";

                    return RedirectToAction(
                        nameof(ConfirmarPagoMatricula),
                        new { idCurso = modelo.IdCurso });
                }

                double distanciaCalculada = CalcularDistanciaKm(
                    (double)ubicacion.Latitud,
                    (double)ubicacion.Longitud,
                    (double)ubicacion.Zona.LatitudCentro.Value,
                    (double)ubicacion.Zona.LongitudCentro.Value);

                if (ubicacion.Zona.RadioMaximoKm.HasValue &&
                    distanciaCalculada >
                    (double)ubicacion.Zona.RadioMaximoKm.Value)
                {
                    TempData["Error"] =
                        "La ubicación está fuera del radio permitido para clases a domicilio.";

                    return RedirectToAction(
                        nameof(ConfirmarPagoMatricula),
                        new { idCurso = modelo.IdCurso });
                }

                distanciaKm = Math.Round(
                    Convert.ToDecimal(distanciaCalculada),
                    2);

                decimal tarifaPorKm =
                    ubicacion.Zona.TarifaPorKm ?? 0;

                decimal costoPorDistancia =
                    distanciaKm * tarifaPorKm;

                costoDesplazamiento = Math.Round(
                    ubicacion.Zona.CostoAdicional +
                    costoPorDistancia,
                    2);

                idUbicacionAlumno =
                    ubicacion.IdUbicacion;
            }

            decimal montoBase = curso.Precio;

            decimal montoTotal =
                montoBase + costoDesplazamiento;

            var pago = new Pago
            {
                IdAlumno = userId,
                IdCurso = curso.IdCurso,

                MontoBase = montoBase,
                CostoDesplazamiento = costoDesplazamiento,
                Monto = montoTotal,

                EsADomicilio = modelo.EsADomicilio,
                IdUbicacionAlumno = idUbicacionAlumno,
                DistanciaKm = distanciaKm,

                TipoPago = "Matricula",
                MetodoPago = "Pendiente",
                Estado = "Pendiente",
                FechaPago = DateTime.Now,
                FechaVencimiento = DateTime.Now.AddDays(3),
                EsManual = false,

                Observaciones = modelo.EsADomicilio
                    ? $"Pago pendiente por matrícula al curso {curso.Nombre}. " +
                      $"Modalidad a domicilio. Desplazamiento: ₡{costoDesplazamiento:N0}."
                    : $"Pago pendiente por matrícula al curso {curso.Nombre}. " +
                      "Modalidad en la academia."
            };

            _context.Pagos.Add(pago);

            await NotificacionHelper.EnviarNotificacionAsync(
                _context,
                _emailService,
                userId,
                categoria: "Pago",
                tipo: "Pago pendiente",
                titulo: "Pago pendiente de matrícula",
                mensaje:
                    $"Se generó un pago pendiente de ₡{montoTotal:N0} " +
                    $"para matricularte en el curso {curso.Nombre}. " +
                    "Adjunta el comprobante para que el administrador pueda revisarlo."
            );

            await _context.SaveChangesAsync();

            // Google Calendar
            if (await _calendarService.TieneTokenAsync(userId) &&
                curso.Horarios != null)
            {
                foreach (var horario in curso.Horarios)
                {
                    await _calendarService.CrearEventoAsync(
                        userId,
                        $"Clase: {curso.Nombre}",
                        modelo.EsADomicilio
                            ? $"Nivel: {curso.Nivel} — Modalidad a domicilio — Pago pendiente."
                            : $"Nivel: {curso.Nivel} — Modalidad en academia — Pago pendiente.",
                        horario.Fecha,
                        horario.HoraInicio,
                        horario.HoraFin);
                }
            }

            TempData["Success"] =
                $"Se generó el pago pendiente por ₡{montoTotal:N0}. " +
                "Debe realizar el pago para completar la matrícula.";

            return RedirectToAction(nameof(HistorialPagos));
        }


        // GET: /Usuario/MisHorarios
        public async Task<IActionResult> MisHorarios(string? buscar, string? dia)
        {
            var query = _context.Horarios
                .Include(h => h.Curso)
                .Where(h => h.Curso.Activo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
                query = query.Where(h => h.Curso.Nombre.Contains(buscar));

            if (!string.IsNullOrWhiteSpace(dia))
                query = query.Where(h => h.DiaSemana == dia);

            var horarios = await query.OrderBy(h => h.DiaSemana)
                                      .ThenBy(h => h.HoraInicio)
                                      .ToListAsync();

            var dias = await _context.Horarios
                .Select(h => h.DiaSemana)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            var viewModel = new UsuarioHorariosViewModel
            {
                FiltroBuscar = buscar,
                FiltroDia = dia,
                DiasDisponibles = dias,
                Horarios = horarios.Select(h => new HorarioUsuarioItemViewModel
                {
                    IdHorario = h.IdHorario,
                    DiaSemana = h.DiaSemana,
                    HoraInicio = h.HoraInicio.ToString(@"hh\:mm"),
                    HoraFin = h.HoraFin.ToString(@"hh\:mm"),
                    NombreCurso = h.Curso.Nombre,
                    Nivel = h.Curso.Nivel,
                    CuposDisponibles = h.Curso.CuposDisponibles
                }).ToList()
            };

            return View("~/Views/Perfiles/UsuarioHorarios.cshtml", viewModel);
        }

        // GET: /Usuario/AgendaPersonal
        public async Task<IActionResult> AgendaPersonal(
            string? dia,
            string? tipo)
        {
            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // 1. Cursos preagendados provenientes de matrículas activas
            var matriculas = await _context.Matriculas
                .Include(m => m.Curso)
                    .ThenInclude(c => c.Horarios)
                .Include(m => m.Curso)
                    .ThenInclude(c => c.Profesor)
                .Where(m =>
                    m.IdAlumno == userId &&
                    m.Estado == "Activa")
                .ToListAsync();

            // 2. Reservas concretas asignadas al alumno
            var reservas = await _context.Reservas
             .Include(r => r.Profesor)
             .Include(r => r.Cancha)
             .Include(r => r.Pagos)
             .Where(r =>
                 r.IdAlumno == userId &&
                 r.Estado == "Asignada")
             .OrderBy(r => r.FechaReserva)
             .ThenBy(r => r.HoraInicio)
             .ToListAsync();


            var idsReservas = reservas
    .Select(r => r.IdReserva)
    .ToList();

            var reprogramacionesPendientes =
                await _context.SolicitudesCurso
                    .Where(s =>
                        s.IdAlumno == userId &&
                        s.IdReservaOrigen.HasValue &&
                        idsReservas.Contains(s.IdReservaOrigen.Value) &&
                        (
                            s.Estado == "Pendiente" ||
                            s.Estado == "En revisión" ||
                            s.Estado == "Propuesta enviada"
                        ))
                    .ToListAsync();
            var clases = new List<AgendaPersonalItemViewModel>();

            // =========================================================
            // CURSOS RECURRENTES / PREAGENDADOS
            // =========================================================

            foreach (var m in matriculas)
            {
                if (m.Curso == null)
                {
                    continue;
                }

                if (m.Curso.Horarios != null &&
                    m.Curso.Horarios.Any())
                {
                    foreach (var h in m.Curso.Horarios)
                    {
                        clases.Add(
                            new AgendaPersonalItemViewModel
                            {
                                IdMatricula = m.IdMatricula,
                                IdCurso = m.Curso.IdCurso,

                                Curso = m.Curso.Nombre,
                                Nivel = m.Curso.Nivel,

                                TipoAgenda = "Curso programado",

                                DiaSemana = h.DiaSemana,

                                FechaClase = string.Empty,
                                FechaClaseReal = null,

                                HoraInicio =
                                    h.HoraInicio.ToString(@"hh\:mm"),

                                HoraFin =
                                    h.HoraFin.ToString(@"hh\:mm"),

                                Profesor =
                                    m.Curso.Profesor == null
                                        ? "Sin profesor asignado"
                                        : $"{m.Curso.Profesor.Nombre} " +
                                          $"{m.Curso.Profesor.Apellidos}",

                                Cancha = "No asignada",

                                EstadoMatricula = m.Estado,
                                Estado = m.Estado
                            });
                    }
                }
                else
                {
                    clases.Add(
                        new AgendaPersonalItemViewModel
                        {
                            IdMatricula = m.IdMatricula,
                            IdCurso = m.Curso.IdCurso,

                            Curso = m.Curso.Nombre,
                            Nivel = m.Curso.Nivel,

                            TipoAgenda = "Curso programado",

                            DiaSemana = "Sin horario asignado",

                            FechaClase = string.Empty,
                            FechaClaseReal = null,

                            HoraInicio = string.Empty,
                            HoraFin = string.Empty,

                            Profesor =
                                m.Curso.Profesor == null
                                    ? "Sin profesor asignado"
                                    : $"{m.Curso.Profesor.Nombre} " +
                                      $"{m.Curso.Profesor.Apellidos}",

                            Cancha = "No asignada",

                            EstadoMatricula = m.Estado,
                            Estado = m.Estado
                        });
                }
            }

            // =========================================================
            // CLASES CON FECHA CONCRETA / RESERVAS
            // =========================================================

            foreach (var r in reservas)
            {

                var pagoSolicitud = r.Pagos
    .FirstOrDefault(p =>
        p.TipoPago == "Solicitud de clase");

                var nombreClase = "Clase reservada";

                if (pagoSolicitud != null &&
                    !string.IsNullOrWhiteSpace(pagoSolicitud.Observaciones))
                {
                    var observaciones = pagoSolicitud.Observaciones;

                    var separador = observaciones.IndexOf(" - ");

                    if (separador >= 0)
                    {
                        nombreClase =
                            observaciones[(separador + 3)..];

                        var puntoModalidad =
                            nombreClase.IndexOf(". Modalidad");

                        if (puntoModalidad >= 0)
                        {
                            nombreClase =
                                nombreClase[..puntoModalidad];
                        }
                    }
                }

                var reprogramacionPendiente =
    reprogramacionesPendientes
        .FirstOrDefault(s =>
            s.IdReservaOrigen == r.IdReserva);


                clases.Add(
                    new AgendaPersonalItemViewModel
                    {
                        IdReserva = r.IdReserva,

                        IdCurso = 0,
                        IdMatricula = 0,

                        Curso = nombreClase,

                        Nivel = string.Empty,

                        TipoAgenda = "Clase reservada",

                        DiaSemana =
                            r.FechaReserva.ToString(
                                "dddd",
                                new System.Globalization.CultureInfo("es-CR")),

                        FechaClase =
                            r.FechaReserva.ToString("dd/MM/yyyy"),

                        FechaClaseReal = r.FechaReserva,

                        HoraInicio =
                            r.HoraInicio.ToString(@"hh\:mm"),

                        HoraFin =
                            r.HoraFin.ToString(@"hh\:mm"),

                        Profesor =
                            r.Profesor == null
                                ? "Sin profesor asignado"
                                : $"{r.Profesor.Nombre} " +
                                  $"{r.Profesor.Apellido}",

                        Cancha =
                            r.Cancha == null
                                ? "Sin cancha asignada"
                                : r.Cancha.Nombre,

                        EstadoMatricula = string.Empty,

                        Estado = r.Estado,

                        TieneReprogramacionPendiente =
                          reprogramacionPendiente != null,

                        EstadoReprogramacion =
                             reprogramacionPendiente?.Estado


                    });
            }

            // =========================================================
            // FILTRO POR DÍA
            // =========================================================

            if (!string.IsNullOrWhiteSpace(dia))
            {
                clases = clases
                    .Where(x =>
                        string.Equals(
                            x.DiaSemana,
                            dia,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                clases = clases
                    .Where(x => x.TipoAgenda == tipo)
                    .ToList();
            }

            var diasDisponibles = clases
                .Select(x => x.DiaSemana)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // Primero las clases con fecha concreta;
            // luego los cursos recurrentes.
            var clasesOrdenadas = clases
                .OrderBy(x => x.FechaClaseReal.HasValue ? 0 : 1)
                .ThenBy(x => x.FechaClaseReal)
                .ThenBy(x => x.DiaSemana)
                .ThenBy(x => x.HoraInicio)
                .ToList();

            var viewModel = new AgendaPersonalViewModel
            {
                FiltroDia = dia,
                FiltroTipo = tipo,
                DiasDisponibles = diasDisponibles,
                Clases = clasesOrdenadas
            };

            return View(
                "~/Views/Matricula/_UsuarioAgenda.cshtml",
                viewModel);
        }


        // POST: /Usuario/CancelarMatricula
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarMatricula(int idMatricula)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth");

            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                    .ThenInclude(c => c.Horarios)
                .FirstOrDefaultAsync(m => m.IdMatricula == idMatricula && m.IdAlumno == userId);

            if (matricula == null)
            {
                TempData["Error"] = "No se encontró la matrícula indicada.";
                return RedirectToAction(nameof(AgendaPersonal));
            }

            matricula.Estado = "Cancelada";

            var curso = await _context.Cursos.FindAsync(matricula.IdCurso);
            if (curso != null) curso.CuposDisponibles += 1;

            await _context.SaveChangesAsync();

            // ── Google Calendar: eliminar eventos del alumno para este curso ──
            if (await _calendarService.TieneTokenAsync(userId) && matricula.Curso?.Horarios != null)
            {
                foreach (var h in matricula.Curso.Horarios.Where(h => h.GoogleEventId != null))
                {
                    await _calendarService.EliminarEventoAsync(userId, h.GoogleEventId!);
                }
            }

            TempData["Success"] = "Matrícula cancelada correctamente.";
            return RedirectToAction(nameof(AgendaPersonal));
        }

        // GET: /Usuario/Reprogramar/5
        public async Task<IActionResult> Reprogramar(int idMatricula)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var matricula = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.IdMatricula == idMatricula && m.IdAlumno == userId);

            if (matricula == null)
            {
                TempData["Error"] = "No se encontró la matrícula indicada.";
                return RedirectToAction(nameof(AgendaPersonal));
            }

            var cursos = await _context.Cursos
                .Include(c => c.Horarios)
                .Include(c => c.Profesor)
                .Where(c => c.Activo && c.IdCurso != matricula.IdCurso)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var viewModel = new UsuarioCursosViewModel
            {
                Cursos = cursos.Select(c => new CursoUsuarioItemViewModel
                {
                    IdCurso = c.IdCurso,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion ?? string.Empty,
                    Nivel = c.Nivel,
                    CuposDisponibles = c.CuposDisponibles,
                    NombreProfesor = c.Profesor != null
                        ? $"{c.Profesor.Nombre} {c.Profesor.Apellidos}"
                        : "Sin asignar",
                    Horarios = c.Horarios != null
                        ? c.Horarios.Select(h =>
                            $"{h.DiaSemana} {h.HoraInicio:hh\\:mm} - {h.HoraFin:hh\\:mm}").ToList()
                        : new List<string>()
                }).ToList()
            };

            ViewBag.IdMatriculaOrigen = idMatricula;

            return View("~/Views/Cursos/Reprogramar.cshtml", viewModel);
        }

        // POST: /Usuario/ConfirmarReprogramacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarReprogramacion(int idMatriculaOrigen, int idCursoNuevo)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var matriculaOrigen = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.IdMatricula == idMatriculaOrigen && m.IdAlumno == userId);

            if (matriculaOrigen == null)
            {
                TempData["Error"] = "No se encontró la matrícula original.";
                return RedirectToAction(nameof(AgendaPersonal));
            }

            var cursoNuevo = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == idCursoNuevo && c.Activo);

            if (cursoNuevo == null)
            {
                TempData["Error"] = "El curso seleccionado no existe o no está activo.";
                return RedirectToAction(nameof(AgendaPersonal));
            }

            if (cursoNuevo.CuposDisponibles <= 0)
            {
                TempData["Error"] = "No hay cupos disponibles en el curso seleccionado.";
                return RedirectToAction(nameof(AgendaPersonal));
            }

            var yaMatriculado = await _context.Matriculas
                .AnyAsync(m => m.IdAlumno == userId &&
                               m.IdCurso == idCursoNuevo &&
                               m.Estado == "Activa");

            if (yaMatriculado)
            {
                TempData["Error"] = "Ya estás matriculado en ese curso.";
                return RedirectToAction(nameof(AgendaPersonal));
            }

            matriculaOrigen.Estado = "Cancelada";
            var cursoOrigen = await _context.Cursos.FindAsync(matriculaOrigen.IdCurso);
            if (cursoOrigen != null)
            {
                cursoOrigen.CuposDisponibles += 1;
            }

            var nuevaMatricula = new Matricula
            {
                IdAlumno = userId,
                IdCurso = idCursoNuevo,
                FechaMatricula = DateTime.Now,
                Estado = "Activa"
            };
            _context.Matriculas.Add(nuevaMatricula);

            cursoNuevo.CuposDisponibles -= 1;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Te reprogramaste exitosamente al curso '{cursoNuevo.Nombre}'.";
            return RedirectToAction(nameof(AgendaPersonal));
        }



        // GET: /Usuario/HistorialPagos USER-05-003 
        public async Task<IActionResult> HistorialPagos(
            string? buscar,
            string? estado,
            string? factura)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var query = _context.Pagos
                .Include(p => p.Factura)
                .Where(p => p.IdAlumno == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(p =>
                    p.TipoPago.Contains(buscar) ||
                    p.MetodoPago.Contains(buscar));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(p => p.Estado == estado);
            }

            if (!string.IsNullOrWhiteSpace(factura))
            {
                query = factura switch
                {
                    "Disponible" => query.Where(p => p.Factura != null),

                    "Pendiente" => query.Where(p =>
                        p.Factura == null &&
                        p.Estado == "Pagado"),

                    "No disponible" => query.Where(p =>
                        p.Factura == null &&
                        p.Estado != "Pagado"),

                    _ => query
                };
            }

            var pagos = await query
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            var model = new UsuarioHistorialPagosViewModel
            {
                FiltroBuscar = buscar,
                FiltroEstado = estado,
                FiltroFactura = factura,

                Pagos = pagos.Select(p => new UsuarioPagoItemViewModel
                {
                    IdPago = p.IdPago,
                    Concepto = p.TipoPago,
                    MetodoPago = p.MetodoPago,
                    Monto = p.Monto,
                    FechaPago = p.FechaPago,
                    FechaFactura = p.Factura != null ? p.Factura.FechaFactura : null,
                    NumeroFactura = p.Factura != null ? p.Factura.NumeroFactura : null,
                    Estado = p.Estado
                }).ToList()
            };

            return View("~/Views/Perfiles/UsuarioHistorialPagos.cshtml", model);
        }


        // GET: /Usuario/DescargarComprobante/ USER-05-004 – Descargar comprobante de pago

        [HttpGet]
        public async Task<IActionResult> DescargarComprobante(int idPago)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var pago = await _context.Pagos
                .Include(p => p.Alumno)
                .Include(p => p.Factura)
                .FirstOrDefaultAsync(p => p.IdPago == idPago && p.IdAlumno == userId);

            if (pago == null)
            {
                TempData["Error"] = "No se encontró el pago seleccionado.";
                return RedirectToAction(nameof(HistorialPagos));
            }

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo-mmp.png");

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(45);
                    page.Size(PageSizes.A4);

                    page.Header().Row(row =>
                    {
                        if (System.IO.File.Exists(logoPath))
                        {
                            row.RelativeItem().Height(70).Image(logoPath).FitHeight();
                        }

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("COMPROBANTE DE PAGO").FontSize(20).Bold();
                            col.Item().Text($"Pago No. PAG-{pago.IdPago}").FontSize(10);
                            col.Item().Text($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy}").FontSize(10);
                        });
                    });

                    page.Content().PaddingTop(30).Column(col =>
                    {
                        col.Spacing(14);

                        col.Item().Text("Datos del alumno").FontSize(14).Bold();
                        col.Item().Text($"Alumno: {pago.Alumno.Nombre} {pago.Alumno.Apellido}");
                        col.Item().Text($"Correo: {pago.Alumno.Email}");

                        col.Item().LineHorizontal(1);

                        col.Item().Text("Detalle del pago").FontSize(14).Bold();
                        col.Item().Text($"Concepto: {pago.TipoPago}");
                        col.Item().Text($"Método de pago: {pago.MetodoPago}");
                        col.Item().Text($"Fecha de pago: {pago.FechaPago:dd/MM/yyyy}");
                        col.Item().Text($"Estado del pago: {pago.Estado}");

                        col.Item().Text($"Monto cancelado: ₡{pago.Monto:N0}")
                            .FontSize(16)
                            .Bold();

                        col.Item().LineHorizontal(1);

                        col.Item().Text("Datos de factura").FontSize(14).Bold();

                        if (pago.Factura != null)
                        {
                            col.Item().Text($"Número de factura: {pago.Factura.NumeroFactura}");
                            col.Item().Text($"Fecha de factura: {pago.Factura.FechaFactura:dd/MM/yyyy}");
                        }
                        else
                        {
                            col.Item().Text("Factura: Pendiente de emisión");
                        }

                        if (!string.IsNullOrWhiteSpace(pago.Observaciones))
                        {
                            col.Item().LineHorizontal(1);
                            col.Item().Text("Observaciones").FontSize(14).Bold();
                            col.Item().Text(pago.Observaciones);
                        }

                        col.Item().PaddingTop(25).Text(
                            "Este documento corresponde a un comprobante de pago generado por el sistema. No sustituye la factura electrónica emitida mediante el sistema del Ministerio de Hacienda."
                        ).FontSize(9).Italic();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Academia de Tennis M.M.P. | Comprobante generado automáticamente")
                        .FontSize(9);
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"Comprobante_PAG-{pago.IdPago}.pdf");
        }

        //// GET: /Usuario/Notificaciones - USER-09-010, USER-09-008, USER-09-009
        //public async Task<IActionResult> Notificaciones()
        //{
        //    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        //    var notificaciones = await _context.Notificaciones
        //        .Where(n => n.IdUsuario == userId)
        //        .OrderByDescending(n => n.FechaEnvio)
        //        .ToListAsync();

        //    var preferencia = await _context.PreferenciasNotificacion
        //        .FirstOrDefaultAsync(p => p.IdUsuario == userId);

        //    var model = new NotificacionesUsuarioViewModel
        //    {
        //        Notificaciones = notificaciones.Select(n => new NotificacionUsuarioItemViewModel
        //        {
        //            IdNotificacion = n.IdNotificacion,
        //            Tipo = n.Tipo,
        //            Titulo = n.Titulo,
        //            Mensaje = n.Mensaje,
        //            Leida = n.Leida,
        //            FechaEnvio = n.FechaEnvio
        //        }).ToList(),

        //        // Si el alumno nunca ha guardado preferencias, se usan los valores por defecto (todo activo, canal Email)
        //        CanalPreferido = preferencia?.CanalPreferido ?? "Email",
        //        NotificacionesPago = preferencia?.NotificacionesPago ?? true,
        //        NotificacionesClase = preferencia?.NotificacionesClase ?? true,
        //        NotificacionesRecordatorio = preferencia?.NotificacionesRecordatorio ?? true,
        //        NotificacionesCampeonato = preferencia?.NotificacionesCampeonato ?? true
        //    };

        //    return View("~/Views/Notificaciones/_NotificacionesUsuario.cshtml", model);
        //}

        //// POST: /Usuario/MarcarNotificacionLeida
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> MarcarNotificacionLeida(int idNotificacion)
        //{
        //    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        //    var notificacion = await _context.Notificaciones
        //        .FirstOrDefaultAsync(n =>
        //            n.IdNotificacion == idNotificacion &&
        //            n.IdUsuario == userId);

        //    if (notificacion == null)
        //    {
        //        TempData["Error"] = "No se encontró la notificación seleccionada.";
        //        return RedirectToAction(nameof(Notificaciones));
        //    }

        //    notificacion.Leida = true;

        //    await _context.SaveChangesAsync();

        //    TempData["MensajeExito"] = "La notificación fue marcada como leída.";

        //    return RedirectToAction(nameof(Notificaciones));
        //}
        //// GET: /Usuario/NotificacionesResumen - resumen para la campana del navbar (USER-09-009)
        //[HttpGet]
        //public async Task<IActionResult> NotificacionesResumen()
        //{
        //    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        //    var noLeidas = await _context.Notificaciones
        //        .CountAsync(n => n.IdUsuario == userId && !n.Leida);

        //    var recientes = await _context.Notificaciones
        //        .Where(n => n.IdUsuario == userId)
        //        .OrderByDescending(n => n.FechaEnvio)
        //        .Take(8)
        //        .Select(n => new
        //        {
        //            id = n.IdNotificacion,
        //            titulo = n.Titulo,
        //            mensaje = n.Mensaje,
        //            leida = n.Leida,
        //            fecha = n.FechaEnvio.ToString("dd/MM/yyyy HH:mm")
        //        })
        //        .ToListAsync();

        //    return Json(new { noLeidas, notificaciones = recientes });
        //}

        //// POST: /Usuario/EliminarNotificacion - USER-09-009
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EliminarNotificacion(int idNotificacion)
        //{
        //    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        //    var notificacion = await _context.Notificaciones
        //        .FirstOrDefaultAsync(n => n.IdNotificacion == idNotificacion && n.IdUsuario == userId);

        //    bool esAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        //    if (notificacion == null)
        //    {
        //        if (esAjax) return Json(new { success = false, mensaje = "No se encontró la notificación." });

        //        TempData["Error"] = "No se encontró la notificación seleccionada.";
        //        return RedirectToAction(nameof(Notificaciones));
        //    }

        //    _context.Notificaciones.Remove(notificacion);
        //    await _context.SaveChangesAsync();

        //    if (esAjax) return Json(new { success = true });

        //    TempData["MensajeExito"] = "La notificación fue eliminada.";
        //    return RedirectToAction(nameof(Notificaciones));
        //}

        //// POST: /Usuario/GuardarPreferenciasNotificacion - USER-09-008 y USER-09-009
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> GuardarPreferenciasNotificacion(
        //    string canalPreferido,
        //    bool notificacionesPago,
        //    bool notificacionesClase,
        //    bool notificacionesRecordatorio,
        //    bool notificacionesCampeonato)
        //{
        //    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        //    var preferencia = await _context.PreferenciasNotificacion
        //        .FirstOrDefaultAsync(p => p.IdUsuario == userId);

        //    if (preferencia == null)
        //    {
        //        preferencia = new PreferenciaNotificacion { IdUsuario = userId };
        //        _context.PreferenciasNotificacion.Add(preferencia);
        //    }

        //    preferencia.CanalPreferido = canalPreferido;
        //    preferencia.NotificacionesPago = notificacionesPago;
        //    preferencia.NotificacionesClase = notificacionesClase;
        //    preferencia.NotificacionesRecordatorio = notificacionesRecordatorio;
        //    preferencia.NotificacionesCampeonato = notificacionesCampeonato;

        //    await _context.SaveChangesAsync();

        //    TempData["MensajeExito"] = "Tus preferencias de notificaciones fueron actualizadas.";
        //    return RedirectToAction(nameof(Notificaciones));
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirComprobante(int idPago, IFormFile comprobante)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var pago = await _context.Pagos
                .FirstOrDefaultAsync(p => p.IdPago == idPago && p.IdAlumno == userId);

            if (pago == null)
            {
                TempData["Error"] = "No se encontró el pago.";
                return RedirectToAction(nameof(HistorialPagos));
            }

            if (comprobante == null || comprobante.Length == 0)
            {
                TempData["Error"] = "Debe adjuntar un comprobante.";
                return RedirectToAction(nameof(HistorialPagos));
            }

            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comprobantes");

            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            var extension = Path.GetExtension(comprobante.FileName);
            var nombreArchivo = $"PAG-{pago.IdPago}-{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await comprobante.CopyToAsync(stream);
            }

            pago.ComprobantePago = "/comprobantes/" + nombreArchivo;
            pago.FechaComprobante = DateTime.Now;
            pago.Estado = "En revisión";
            pago.MetodoPago = "Comprobante adjunto";

            await NotificacionHelper.EnviarNotificacionAsync(
                _context,
                _emailService,
                pago.IdAlumno,
                categoria: "Pago",
                tipo: "Comprobante recibido",
                titulo: "Comprobante recibido",
                mensaje:
                    $"Recibimos el comprobante correspondiente al pago PAG-{pago.IdPago}. " +
                    "El pago quedó en revisión y será validado por un administrador."
            );

            await _context.SaveChangesAsync();

            TempData["Success"] = "Comprobante adjuntado correctamente. Queda pendiente de revisión.";
            return RedirectToAction(nameof(HistorialPagos));
        }

        private static double CalcularDistanciaKm(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
        {
            const double radioTierra = 6371;

            double dLat =
                (lat2 - lat1) * Math.PI / 180;

            double dLon =
                (lon2 - lon1) * Math.PI / 180;

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) *
                Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            double c =
                2 * Math.Atan2(
                    Math.Sqrt(a),
                    Math.Sqrt(1 - a));

            return radioTierra * c;
        }

        private static bool MatriculaCerrada(
            Curso curso,
            int horasAnticipacion = 3)
        {
            if (curso.Horarios == null ||
                !curso.Horarios.Any())
            {
                return false;
            }

            DateTime ahora = DateTime.Now;

            DateTime primerInicio = curso.Horarios
                .Select(h =>
                    h.Fecha.Date.Add(h.HoraInicio))
                .Min();

            DateTime fechaLimite =
                primerInicio.AddHours(-horasAnticipacion);

            return ahora >= fechaLimite;
        }
    }

}