using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models;
using System.Globalization;

namespace ProyectoGrupalTennis.Controllers
{
    public class GeolocalizacionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GeolocalizacionController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AdminGeolocalizacion
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var zonas = await _context.ZonasCobertura
                .OrderBy(z => z.Nombre)
                .ToListAsync();

            return View(zonas);
        }

        // GET: AdminGeolocalizacion/Crear
        [Authorize(Roles = "Administrador")]
        public IActionResult Crear()
        {
            return View(new ZonaCobertura());
        }

        // POST: AdminGeolocalizacion/Crear
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
    ZonaCobertura zona,
    string? LatitudCentroTexto,
    string? LongitudCentroTexto)
        {
            ModelState.Remove(nameof(zona.LatitudCentro));
            ModelState.Remove(nameof(zona.LongitudCentro));

            const NumberStyles estilosCoordenada =
                NumberStyles.Float |
                NumberStyles.AllowLeadingSign;

            if (decimal.TryParse(
                LatitudCentroTexto,
                estilosCoordenada,
                CultureInfo.InvariantCulture,
                out decimal latitud))
            {
                zona.LatitudCentro = latitud;
            }
            else
            {
                ModelState.AddModelError(
                    nameof(zona.LatitudCentro),
                    "La latitud seleccionada no es válida.");
            }

            if (decimal.TryParse(
                LongitudCentroTexto,
                estilosCoordenada,
                CultureInfo.InvariantCulture,
                out decimal longitud))
            {
                zona.LongitudCentro = longitud;
            }
            else
            {
                ModelState.AddModelError(
                    nameof(zona.LongitudCentro),
                    "La longitud seleccionada no es válida.");
            }

            if (!zona.LatitudCentro.HasValue ||
                !zona.LongitudCentro.HasValue)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Debe seleccionar una ubicación en el mapa.");
            }
            else
            {
                if (zona.LatitudCentro.Value < -90 ||
                    zona.LatitudCentro.Value > 90)
                {
                    ModelState.AddModelError(
                        nameof(zona.LatitudCentro),
                        "La latitud seleccionada no es válida.");
                }

                if (zona.LongitudCentro.Value < -180 ||
                    zona.LongitudCentro.Value > 180)
                {
                    ModelState.AddModelError(
                        nameof(zona.LongitudCentro),
                        "La longitud seleccionada no es válida.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(zona);
            }

            zona.Nombre = zona.Nombre.Trim();

            
            bool nombreRepetido =
                await _context.ZonasCobertura
                    .AnyAsync(z =>
                        z.Nombre.ToLower() ==
                        zona.Nombre.ToLower());

            if (nombreRepetido)
            {
                ModelState.AddModelError(
                    nameof(zona.Nombre),
                    "Ya existe una zona de cobertura con ese nombre.");
            }

            if (zona.CostoAdicional < 0)
            {
                ModelState.AddModelError(
                    nameof(zona.CostoAdicional),
                    "El costo adicional no puede ser negativo.");
            }

            if (zona.RadioMaximoKm.HasValue &&
                zona.RadioMaximoKm.Value <= 0)
            {
                ModelState.AddModelError(
                    nameof(zona.RadioMaximoKm),
                    "El radio máximo debe ser mayor que cero.");
            }

            if (zona.TarifaPorKm.HasValue &&
                zona.TarifaPorKm.Value < 0)
            {
                ModelState.AddModelError(
                    nameof(zona.TarifaPorKm),
                    "La tarifa por kilómetro no puede ser negativa.");
            }

            if (!ModelState.IsValid)
            {
                return View(zona);
            }

            _context.ZonasCobertura.Add(zona);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La zona de cobertura se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: AdminGeolocalizacion/Editar/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int id)
        {
            var zona = await _context.ZonasCobertura
                .FirstOrDefaultAsync(z => z.IdZona == id);

            if (zona == null)
            {
                return NotFound();
            }

            return View(zona);
        }

        // POST: AdminGeolocalizacion/Editar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            ZonaCobertura zona,
            string? LatitudCentroTexto,
            string? LongitudCentroTexto)
        {
            if (id != zona.IdZona)
            {
                return BadRequest();
            }
            ModelState.Remove(nameof(zona.LatitudCentro));
            ModelState.Remove(nameof(zona.LongitudCentro));

            const NumberStyles estilosCoordenada =
                NumberStyles.Float |
                NumberStyles.AllowLeadingSign;

            bool latitudValida =
                decimal.TryParse(
                    LatitudCentroTexto,
                    estilosCoordenada,
                    CultureInfo.InvariantCulture,
                    out decimal latitud);

            bool longitudValida =
                decimal.TryParse(
                    LongitudCentroTexto,
                    estilosCoordenada,
                    CultureInfo.InvariantCulture,
                    out decimal longitud);

            if (!latitudValida)
            {
                ModelState.AddModelError(
                    nameof(zona.LatitudCentro),
                    "La latitud seleccionada no es válida.");
            }
            else
            {
                zona.LatitudCentro = latitud;
            }

            if (!longitudValida)
            {
                ModelState.AddModelError(
                    nameof(zona.LongitudCentro),
                    "La longitud seleccionada no es válida.");
            }
            else
            {
                zona.LongitudCentro = longitud;
            }
            if (!zona.LatitudCentro.HasValue ||
            !zona.LongitudCentro.HasValue)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Debe seleccionar una ubicación en el mapa.");
            }
            else
            {
                if (zona.LatitudCentro.Value < -90 ||
                    zona.LatitudCentro.Value > 90)
                {
                    ModelState.AddModelError(
                        nameof(zona.LatitudCentro),
                        "La latitud seleccionada no es válida.");
                }

                if (zona.LongitudCentro.Value < -180 ||
                    zona.LongitudCentro.Value > 180)
                {
                    ModelState.AddModelError(
                        nameof(zona.LongitudCentro),
                        "La longitud seleccionada no es válida.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(zona);
            }

            bool nombreRepetido = await _context.ZonasCobertura
                .AnyAsync(z =>
                    z.IdZona != zona.IdZona &&
                    z.Nombre.ToLower() == zona.Nombre.ToLower());

            if (nombreRepetido)
            {
                ModelState.AddModelError(
                    nameof(zona.Nombre),
                    "Ya existe otra zona con ese nombre.");
            }

            if (zona.CostoAdicional < 0)
            {
                ModelState.AddModelError(
                    nameof(zona.CostoAdicional),
                    "El costo adicional no puede ser negativo.");
            }

            if (zona.RadioMaximoKm.HasValue &&
                zona.RadioMaximoKm.Value <= 0)
            {
                ModelState.AddModelError(
                    nameof(zona.RadioMaximoKm),
                    "El radio máximo debe ser mayor que cero.");
            }

            if (zona.TarifaPorKm.HasValue &&
                zona.TarifaPorKm.Value < 0)
            {
                ModelState.AddModelError(
                    nameof(zona.TarifaPorKm),
                    "La tarifa por kilómetro no puede ser negativa.");
            }

            if (!ModelState.IsValid)
            {
                return View(zona);
            }

            var zonaExistente = await _context.ZonasCobertura
                .FirstOrDefaultAsync(z => z.IdZona == id);

            if (zonaExistente == null)
            {
                return NotFound();
            }

            zonaExistente.Nombre = zona.Nombre.Trim();
            zonaExistente.CostoAdicional = zona.CostoAdicional;
            zonaExistente.LatitudCentro = zona.LatitudCentro;
            zonaExistente.LongitudCentro = zona.LongitudCentro;
            zonaExistente.RadioMaximoKm = zona.RadioMaximoKm;
            zonaExistente.TarifaPorKm = zona.TarifaPorKm;
            zonaExistente.Activa = zona.Activa;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "La zona de cobertura se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // POST: AdminGeolocalizacion/CambiarEstado/5
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var zona = await _context.ZonasCobertura
                .FirstOrDefaultAsync(z => z.IdZona == id);

            if (zona == null)
            {
                return NotFound();
            }

            zona.Activa = !zona.Activa;

            await _context.SaveChangesAsync();

            TempData["Success"] = zona.Activa
                ? "La zona fue activada correctamente."
                : "La zona fue desactivada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: AdminGeolocalizacion/Detalles/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Detalles(int id)
        {
            var zona = await _context.ZonasCobertura
                .Include(z => z.Ubicaciones)
                .FirstOrDefaultAsync(z => z.IdZona == id);

            if (zona == null)
            {
                return NotFound();
            }

            return View(zona);
        }

        // ---Alumno---

        // GET: Geolocalizacion/MiUbicacion
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> MiUbicacion()
        {
            string? idAlumno = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(idAlumno))
            {
                return Challenge();
            }

            var ubicacion = await _context.UbicacionesAlumno
                .Include(u => u.Zona)
                .FirstOrDefaultAsync(u =>
                    u.IdAlumno == idAlumno &&
                    u.EsPrincipal);

            var zonasActivas = await _context.ZonasCobertura
                .Where(z => z.Activa)
                .OrderBy(z => z.Nombre)
                .ToListAsync();

            ViewBag.Zonas = zonasActivas;

            if (ubicacion == null)
            {
                ubicacion = new UbicacionAlumno
                {
                    IdAlumno = idAlumno,
                    EsPrincipal = true
                };
            }

            return View(ubicacion);
        }

        // POST: Geolocalizacion/MiUbicacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> MiUbicacion(
            UbicacionAlumno modelo,
            string? LatitudTexto,
            string? LongitudTexto)
        {
            string? idAlumno = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(idAlumno))
            {
                return Challenge();
            }

            modelo.IdAlumno = idAlumno;
            modelo.EsPrincipal = true;

            ModelState.Remove(nameof(modelo.Latitud));
            ModelState.Remove(nameof(modelo.Longitud));

            const NumberStyles estilosCoordenada =
                NumberStyles.Float |
                NumberStyles.AllowLeadingSign;

            if (decimal.TryParse(
                LatitudTexto,
                estilosCoordenada,
                CultureInfo.InvariantCulture,
                out decimal latitud))
            {
                modelo.Latitud = latitud;
            }
            else
            {
                ModelState.AddModelError(
                    nameof(modelo.Latitud),
                    "La latitud seleccionada no es válida.");
            }

            if (decimal.TryParse(
                LongitudTexto,
                estilosCoordenada,
                CultureInfo.InvariantCulture,
                out decimal longitud))
            {
                modelo.Longitud = longitud;
            }
            else
            {
                ModelState.AddModelError(
                    nameof(modelo.Longitud),
                    "La longitud seleccionada no es válida.");
            }

            // Evita errores de validación por propiedades que no vienen del formulario.
            ModelState.Remove(nameof(modelo.IdAlumno));
            ModelState.Remove(nameof(modelo.Alumno));
            ModelState.Remove(nameof(modelo.Zona));
            ModelState.Remove(nameof(modelo.EsPrincipal));

            if (!modelo.IdZona.HasValue)
            {
                ModelState.AddModelError(
                    nameof(modelo.IdZona),
                    "La ubicación seleccionada está fuera de la zona de cobertura.");
            }
            else
            {
                bool zonaValida = await _context.ZonasCobertura
                    .AnyAsync(z =>
                        z.IdZona == modelo.IdZona.Value &&
                        z.Activa);

                if (!zonaValida)
                {
                    ModelState.AddModelError(
                        nameof(modelo.IdZona),
                        "La zona detectada no está disponible.");
                }
            }

            if (string.IsNullOrWhiteSpace(modelo.DireccionCompleta))
            {
                ModelState.AddModelError(
                    nameof(modelo.DireccionCompleta),
                    "Debe ingresar una dirección o referencia.");
            }

            if (modelo.Latitud < -90 || modelo.Latitud > 90)
            {
                ModelState.AddModelError(
                    nameof(modelo.Latitud),
                    "La latitud seleccionada no es válida.");
            }

            if (modelo.Longitud < -180 || modelo.Longitud > 180)
            {
                ModelState.AddModelError(
                    nameof(modelo.Longitud),
                    "La longitud seleccionada no es válida.");
            }

            if (modelo.Latitud == 0 && modelo.Longitud == 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Debe seleccionar una ubicación en el mapa.");
            }      

            if (!ModelState.IsValid)
            {
                ViewBag.Zonas = await _context.ZonasCobertura
                    .Where(z => z.Activa)
                    .OrderBy(z => z.Nombre)
                    .ToListAsync();

                return View(modelo);
            }

            var ubicacionExistente = await _context.UbicacionesAlumno
                .FirstOrDefaultAsync(u =>
                    u.IdAlumno == idAlumno &&
                    u.EsPrincipal);

            if (ubicacionExistente == null)
            {
                var nuevaUbicacion = new UbicacionAlumno
                {
                    IdAlumno = idAlumno,
                    DireccionCompleta = modelo.DireccionCompleta.Trim(),
                    Latitud = modelo.Latitud,
                    Longitud = modelo.Longitud,
                    IdZona = modelo.IdZona,
                    EsPrincipal = true
                };

                _context.UbicacionesAlumno.Add(nuevaUbicacion);
            }
            else
            {
                ubicacionExistente.DireccionCompleta =
                    modelo.DireccionCompleta.Trim();

                ubicacionExistente.Latitud = modelo.Latitud;
                ubicacionExistente.Longitud = modelo.Longitud;
                ubicacionExistente.IdZona = modelo.IdZona;
                ubicacionExistente.EsPrincipal = true;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Tu ubicación se guardó correctamente.";

            return RedirectToAction(nameof(IndexAlumno));
        }
        // GET: Geolocalizacion/IndexAlumno
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> IndexAlumno()
        {
            string? idAlumno = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(idAlumno))
            {
                return Challenge();
            }

            var ubicacion = await _context.UbicacionesAlumno
                .Include(u => u.Zona)
                .FirstOrDefaultAsync(u =>
                    u.IdAlumno == idAlumno &&
                    u.EsPrincipal);

            return View(ubicacion);
        }

        [HttpGet]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ObtenerZona(
    string? latitud,
    string? longitud)
        {
            const NumberStyles estilosCoordenada =
                NumberStyles.Float |
                NumberStyles.AllowLeadingSign;

            bool latitudValida =
                decimal.TryParse(
                    latitud,
                    estilosCoordenada,
                    CultureInfo.InvariantCulture,
                    out decimal latitudDecimal);

            bool longitudValida =
                decimal.TryParse(
                    longitud,
                    estilosCoordenada,
                    CultureInfo.InvariantCulture,
                    out decimal longitudDecimal);

            if (!latitudValida || !longitudValida)
            {
                return Json(new
                {
                    encontrada = false,
                    error = "Las coordenadas seleccionadas no son válidas."
                });
            }

            var zonas = await _context.ZonasCobertura
                .Where(z =>
                    z.Activa &&
                    z.LatitudCentro.HasValue &&
                    z.LongitudCentro.HasValue)
                .ToListAsync();

            ZonaCobertura? zonaMasCercana = null;
            double menorDistancia = double.MaxValue;

            foreach (var zona in zonas)
            {
                double distancia = CalcularDistanciaKm(
                    (double)latitudDecimal,
                    (double)longitudDecimal,
                    (double)zona.LatitudCentro!.Value,
                    (double)zona.LongitudCentro!.Value);

                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    zonaMasCercana = zona;
                }
            }

            if (zonaMasCercana == null)
            {
                return Json(new
                {
                    encontrada = false
                });
            }

            bool dentroRadio =
                !zonaMasCercana.RadioMaximoKm.HasValue ||
                menorDistancia <=
                (double)zonaMasCercana.RadioMaximoKm.Value;

            decimal distanciaDecimal =
                Convert.ToDecimal(menorDistancia);

            decimal costoFijo =
                zonaMasCercana.CostoAdicional;

            decimal tarifaPorKm =
                zonaMasCercana.TarifaPorKm ?? 0;

            decimal costoPorDistancia =
                distanciaDecimal * tarifaPorKm;

            decimal costoDesplazamiento =
                costoFijo + costoPorDistancia;

            costoDesplazamiento =
                Math.Round(costoDesplazamiento, 2);

            return Json(new
            {
                encontrada = true,

                idZona =
                    zonaMasCercana.IdZona,

                nombre =
                    zonaMasCercana.Nombre,

                costo =
                    zonaMasCercana.CostoAdicional,

                radio =
                    zonaMasCercana.RadioMaximoKm,

                tarifaKm =
                    zonaMasCercana.TarifaPorKm,

                distancia =
                    Math.Round(menorDistancia, 2),

                costoPorDistancia =
                    Math.Round(costoPorDistancia, 2),

                costoDesplazamiento,

                dentroRadio
            });
        }

        // GET: Geolocalizacion/ClasesDomicilioProfesor
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> ClasesDomicilioProfesor(
            string? curso)
        {
            string? userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var profesor = await _context.Profesores
                .FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    p.Activo);

            var viewModel = new ClasesDomicilioProfesorViewModel
            {
                FiltroCurso = curso
            };

            if (profesor == null)
            {
                TempData["Error"] =
                    "Tu cuenta no tiene un perfil de profesor vinculado.";

                return View(viewModel);
            }

            var query = _context.Matriculas
                .AsNoTracking()
                .Include(m => m.Alumno)
                .Include(m => m.Curso)
                    .ThenInclude(c => c.Horarios)
                .Include(m => m.UbicacionAlumno)
                    .ThenInclude(u => u.Zona)
                .Where(m =>
                    m.Estado == "Activa" &&
                    m.EsADomicilio &&
                    m.Curso.IdProfesor == profesor.Id &&
                    m.UbicacionAlumno != null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(curso))
            {
                query = query.Where(m =>
                    m.Curso.Nombre == curso);
            }

            var matriculas = await query
                .OrderBy(m => m.Curso.Nombre)
                .ThenBy(m => m.Alumno.Nombre)
                .ToListAsync();

            viewModel.CursosDisponibles = await _context.Cursos
                .Where(c =>
                    c.Activo &&
                    c.IdProfesor == profesor.Id)
                .OrderBy(c => c.Nombre)
                .Select(c => c.Nombre)
                .ToListAsync();

            viewModel.Clases = matriculas
                .Select(m => new ClaseDomicilioProfesorItemViewModel
                {
                    IdMatricula = m.IdMatricula,
                    IdCurso = m.IdCurso,

                    Curso = m.Curso.Nombre,
                    Nivel = m.Curso.Nivel,

                    Alumno =
                        $"{m.Alumno.Nombre} {m.Alumno.Apellido}",

                    Correo =
                        m.Alumno.Email ?? "No registrado",

                    Telefono =
                        m.Alumno.PhoneNumber ?? "No registrado",

                    Direccion =
                        m.UbicacionAlumno!.DireccionCompleta,

                    Zona =
                        m.UbicacionAlumno.Zona?.Nombre
                        ?? "Sin zona asignada",

                    Latitud =
                        m.UbicacionAlumno.Latitud,

                    Longitud =
                        m.UbicacionAlumno.Longitud,

                    DistanciaKm =
                        m.DistanciaKm,

                    Horarios = m.Curso.Horarios
                        .OrderBy(h => h.Fecha)
                        .ThenBy(h => h.HoraInicio)
                        .Select(h =>
                            $"{h.Fecha:dd/MM/yyyy} ({h.DiaSemana}) " +
                            $"{h.HoraInicio:hh\\:mm} - " +
                            $"{h.HoraFin:hh\\:mm}")
                        .ToList()
                })
                .ToList();

            return View(viewModel);
        }

        // GET: Geolocalizacion/MapaLogisticoAdmin
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> MapaLogisticoAdmin(
            int? profesor,
            int? zona,
            DateTime? fecha)
        {
            var viewModel = new MapaLogisticoAdminViewModel
            {
                FiltroProfesor = profesor,
                FiltroZona = zona,
                FiltroFecha = fecha
            };

            // Opciones para el filtro de profesores
            viewModel.Profesores = await _context.Profesores
                .AsNoTracking()
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ThenBy(p => p.Apellidos)
                .Select(p => new OpcionProfesorMapaViewModel
                {
                    IdProfesor = p.Id,
                    Nombre = p.Nombre + " " + p.Apellidos
                })
                .ToListAsync();

            // Opciones para el filtro de zonas
            viewModel.Zonas = await _context.ZonasCobertura
                .AsNoTracking()
                .Where(z => z.Activa)
                .OrderBy(z => z.Nombre)
                .Select(z => new OpcionZonaMapaViewModel
                {
                    IdZona = z.IdZona,
                    Nombre = z.Nombre
                })
                .ToListAsync();

            var query = _context.Matriculas
                .AsNoTracking()
                .Include(m => m.Alumno)
                .Include(m => m.UbicacionAlumno)
                    .ThenInclude(u => u.Zona)
                .Include(m => m.Curso)
                    .ThenInclude(c => c.Profesor)
                .Include(m => m.Curso)
                    .ThenInclude(c => c.Horarios)
                .Where(m =>
                    m.Estado == "Activa" &&
                    m.EsADomicilio &&
                    m.UbicacionAlumno != null &&
                    m.Curso.Profesor != null)
                .AsQueryable();

            if (profesor.HasValue)
            {
                query = query.Where(m =>
                    m.Curso.IdProfesor == profesor.Value);
            }

            if (zona.HasValue)
            {
                query = query.Where(m =>
                    m.UbicacionAlumno!.IdZona == zona.Value);
            }

            var matriculas = await query
                .OrderBy(m => m.Curso.Nombre)
                .ThenBy(m => m.Alumno.Nombre)
                .ToListAsync();

            var clases = new List<ClaseMapaAdminItemViewModel>();

            foreach (var matricula in matriculas)
            {
                if (matricula.Curso == null ||
                    matricula.Alumno == null ||
                    matricula.UbicacionAlumno == null)
                {
                    continue;
                }

                var horarios = matricula.Curso.Horarios?
                    .AsEnumerable()
                    ?? Enumerable.Empty<Horario>();

                if (fecha.HasValue)
                {
                    horarios = horarios.Where(h =>
                        h.Fecha.Date == fecha.Value.Date);
                }

                foreach (var horario in horarios)
                {
                    clases.Add(new ClaseMapaAdminItemViewModel
                    {
                        IdMatricula = matricula.IdMatricula,
                        IdCurso = matricula.IdCurso,

                        IdProfesor =
                            matricula.Curso.IdProfesor ?? 0,

                        IdZona =
                            matricula.UbicacionAlumno.IdZona,

                        Alumno =
                            $"{matricula.Alumno.Nombre} " +
                            $"{matricula.Alumno.Apellido}",

                        Profesor =
                            matricula.Curso.Profesor != null
                                ? $"{matricula.Curso.Profesor.Nombre} " +
                                  $"{matricula.Curso.Profesor.Apellidos}"
                                : "Sin profesor asignado",

                        Curso = matricula.Curso.Nombre,

                        Nivel = matricula.Curso.Nivel,

                        Direccion =
                            matricula.UbicacionAlumno.DireccionCompleta,

                        Zona =
                            matricula.UbicacionAlumno.Zona?.Nombre
                            ?? "Sin zona asignada",

                        Latitud =
                            matricula.UbicacionAlumno.Latitud,

                        Longitud =
                            matricula.UbicacionAlumno.Longitud,

                        Fecha = horario.Fecha,

                        DiaSemana = horario.DiaSemana,

                        HoraInicio =
                            horario.HoraInicio.ToString(@"hh\:mm"),

                        HoraFin =
                            horario.HoraFin.ToString(@"hh\:mm"),

                        TelefonoAlumno =
                            matricula.Alumno.PhoneNumber
                            ?? "No registrado"
                    });
                }
            }

            viewModel.Clases = clases
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.HoraInicio)
                .ThenBy(c => c.Profesor)
                .ToList();

            return View(viewModel);
        }

        private static double CalcularDistanciaKm(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
        {
            const double radioTierra = 6371;

            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;

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


    }
}