using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Models.ViewModels;
using ProyectoGrupalTennis.Services;
using System.Security.Claims;

namespace ProyectoGrupalTennis.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ChatbotService _chatbotService;

        public ChatbotController(AppDbContext context, ChatbotService chatbotService)
        {
            _context = context;
            _chatbotService = chatbotService;
        }

        #region Widget público (USER-06-001 / 002 / 003)

        // GET: /Chatbot/PreguntasRapidas
        [HttpGet]
        public async Task<IActionResult> PreguntasRapidas()
        {
            var preguntas = await _chatbotService.ObtenerPreguntasRapidasAsync();
            return Json(preguntas);
        }

        // POST: /Chatbot/Preguntar
        [HttpPost]
        public async Task<IActionResult> Preguntar([FromBody] ChatMensajeRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Mensaje))
            {
                return BadRequest();
            }

            var idUsuario = User?.Identity?.IsAuthenticated == true
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var respuesta = await _chatbotService.ProcesarMensajeAsync(request.Mensaje, idUsuario);

            return Json(respuesta);
        }

        #endregion

        #region Administración de FAQ - ADM-06-001

        // GET: /Chatbot/AdminFaqs
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AdminFaqs(string? buscar, string? categoria)
        {
            var query = _context.PreguntasFrecuentes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(p =>
                    p.Pregunta.Contains(buscar) || p.Respuesta.Contains(buscar));
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                query = query.Where(p => p.Categoria == categoria);
            }

            var preguntas = await query
                .OrderBy(p => p.Categoria)
                .ThenBy(p => p.Pregunta)
                .ToListAsync();

            var model = new AdminFaqIndexViewModel
            {
                FiltroBuscar = buscar,
                FiltroCategoria = categoria,
                MensajeExito = TempData["MensajeExito"]?.ToString(),
                MensajeError = TempData["MensajeError"]?.ToString(),
                Preguntas = preguntas.Select(p => new AdminFaqItemViewModel
                {
                    IdPregunta = p.IdPregunta,
                    Pregunta = p.Pregunta,
                    Respuesta = p.Respuesta,
                    Categoria = p.Categoria,
                    Activa = p.Activa,
                    VecesConsultada = p.VecesConsultada
                }).ToList()
            };

            return View("~/Views/Perfiles/AdminChatbotFaqs.cshtml", model);
        }

        // POST: /Chatbot/GuardarFaq  (crea si IdPregunta == 0, edita si ya existe)
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarFaq(AdminFaqItemViewModel model)
        {
            if (!ModelState.IsValid ||
                string.IsNullOrWhiteSpace(model.Pregunta) ||
                string.IsNullOrWhiteSpace(model.Respuesta))
            {
                TempData["MensajeError"] = "La pregunta, la respuesta y la categoría son obligatorias.";
                return RedirectToAction(nameof(AdminFaqs));
            }

            if (model.IdPregunta == 0)
            {
                // ADM-06-001 criterio 1: carga de nueva FAQ
                _context.PreguntasFrecuentes.Add(new PreguntaFrecuente
                {
                    Pregunta = model.Pregunta.Trim(),
                    Respuesta = model.Respuesta.Trim(),
                    Categoria = model.Categoria,
                    Activa = true
                });

                TempData["MensajeExito"] = "Pregunta frecuente agregada correctamente.";
            }
            else
            {
                var faq = await _context.PreguntasFrecuentes
                    .FirstOrDefaultAsync(p => p.IdPregunta == model.IdPregunta);

                if (faq == null)
                {
                    TempData["MensajeError"] = "La pregunta que intentas editar ya no existe.";
                    return RedirectToAction(nameof(AdminFaqs));
                }

                // ADM-06-001 criterio 2: actualizar respuesta existente
                faq.Pregunta = model.Pregunta.Trim();
                faq.Respuesta = model.Respuesta.Trim();
                faq.Categoria = model.Categoria;

                TempData["MensajeExito"] = "Pregunta frecuente actualizada correctamente.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AdminFaqs));
        }

        // POST: /Chatbot/CambiarEstadoFaq - activa/desactiva sin borrar el historial
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoFaq(int idPregunta)
        {
            var faq = await _context.PreguntasFrecuentes
                .FirstOrDefaultAsync(p => p.IdPregunta == idPregunta);

            if (faq == null)
            {
                TempData["MensajeError"] = "No se encontró la pregunta.";
                return RedirectToAction(nameof(AdminFaqs));
            }

            faq.Activa = !faq.Activa;
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = faq.Activa
                ? "La pregunta fue activada."
                : "La pregunta fue desactivada y el bot dejará de sugerirla.";

            return RedirectToAction(nameof(AdminFaqs));
        }

        // POST: /Chatbot/EliminarFaq - ADM-06-001 criterio 3
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarFaq(int idPregunta)
        {
            var faq = await _context.PreguntasFrecuentes
                .FirstOrDefaultAsync(p => p.IdPregunta == idPregunta);

            if (faq == null)
            {
                TempData["MensajeError"] = "No se encontró la pregunta.";
                return RedirectToAction(nameof(AdminFaqs));
            }

            _context.PreguntasFrecuentes.Remove(faq);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Pregunta frecuente eliminada. El bot ya no responderá sobre ese tema.";
            return RedirectToAction(nameof(AdminFaqs));
        }

        #endregion

        #region Consultas fallidas - ADM-06-003

        // GET: /Chatbot/AdminConsultasFallidas
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AdminConsultasFallidas()
        {
            var todas = await _context.ConsultasChatbot
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaConsulta)
                .ToListAsync();

            var noResueltas = todas.Where(c => !c.Resuelto).ToList();

            var ranking = noResueltas
                .GroupBy(c => c.MensajeUsuario.Trim().ToLowerInvariant())
                .Select(g => new FraseFrecuenteViewModel
                {
                    Frase = g.First().MensajeUsuario,
                    Cantidad = g.Count()
                })
                .OrderByDescending(f => f.Cantidad)
                .Take(15)
                .ToList();

            var model = new AdminConsultasFallidasIndexViewModel
            {
                TotalConsultas = todas.Count,
                TotalNoResueltas = noResueltas.Count,
                TotalRedirigidasWhatsapp = noResueltas.Count,
                RankingFrasesSinRespuesta = ranking,
                Consultas = noResueltas.Take(100).Select(c => new AdminConsultaFallidaViewModel
                {
                    IdConsulta = c.IdConsulta,
                    MensajeUsuario = c.MensajeUsuario,
                    FechaConsulta = c.FechaConsulta,
                    Usuario = c.Usuario != null
                        ? $"{c.Usuario.Nombre} {c.Usuario.Apellido}"
                        : "Invitado"
                }).ToList()
            };

            return View("~/Views/Perfiles/AdminChatbotConsultas.cshtml", model);
        }

        // GET: /Chatbot/ExportarConsultasFallidas - ADM-06-003 criterio 3
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ExportarConsultasFallidas()
        {
            var noResueltas = await _context.ConsultasChatbot
                .Include(c => c.Usuario)
                .Where(c => !c.Resuelto)
                .OrderByDescending(c => c.FechaConsulta)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Consultas sin respuesta");

            worksheet.Cell(1, 1).Value = "CONSULTAS DEL ASISTENTE VIRTUAL SIN RESPUESTA";
            worksheet.Range(1, 1, 1, 4).Merge().Style.Font.Bold = true;
            worksheet.Range(1, 1, 1, 4).Style.Font.FontSize = 14;

            worksheet.Cell(2, 1).Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet.Range(2, 1, 2, 4).Merge();

            worksheet.Cell(4, 1).Value = "Fecha";
            worksheet.Cell(4, 2).Value = "Hora";
            worksheet.Cell(4, 3).Value = "Usuario";
            worksheet.Cell(4, 4).Value = "Consulta sin respuesta";

            var fila = 5;
            foreach (var consulta in noResueltas)
            {
                worksheet.Cell(fila, 1).Value = consulta.FechaConsulta.ToString("dd/MM/yyyy");
                worksheet.Cell(fila, 2).Value = consulta.FechaConsulta.ToString("HH:mm");
                worksheet.Cell(fila, 3).Value = consulta.Usuario != null
                    ? $"{consulta.Usuario.Nombre} {consulta.Usuario.Apellido}"
                    : "Invitado";
                worksheet.Cell(fila, 4).Value = consulta.MensajeUsuario;
                fila++;
            }

            var headerRange = worksheet.Range(4, 1, 4, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#A3C644");
            headerRange.Style.Font.FontColor = XLColor.White;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Consultas_Chatbot_SinRespuesta_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        #endregion
    }
}
