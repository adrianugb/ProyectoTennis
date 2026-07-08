using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.EntityFrameworkCore;
using ProyectoGrupalTennis.Services;

namespace ProyectoGrupalTennis.Helpers
{
    // USER-09-009: punto unico para crear notificaciones.
    // USER-09-008: ademas de guardar la notificacion en la app (siempre),
    // envia tambien un correo real si el canal preferido del alumno es "Email".
    // Canales soportados actualmente: "Email" y "Push" (bandeja dentro de la plataforma).
    // SMS/WhatsApp no estan disponibles todavia.
    public static class NotificacionHelper
    {
        /// <summary>
        /// Crea una notificacion para el usuario, siempre y cuando:
        /// 1) el alumno no haya desactivado esa categoria de aviso, y
        /// 2) no exista ya una notificacion identica muy reciente (evita duplicados).
        /// Categorias validas: "Pago", "Clase", "Recordatorio", "Campeonato".
        /// Cualquier otra categoria (por ejemplo "General") no se puede desactivar
        /// desde las preferencias del alumno.
        ///
        /// La notificacion siempre queda guardada dentro de la plataforma (ese es el
        /// respaldo/canal alterno). Si el canal preferido del alumno es "Email", el
        /// sistema intenta ademas enviar un correo real:
        ///   - si el correo se envia con exito, CanalUsado queda en "Email".
        ///   - si el correo falla (SMTP caido, credenciales, alumno sin correo, etc.),
        ///     se deja constancia del error (EnvioFallido = true) y la notificacion
        ///     sigue disponible por el medio alterno (la plataforma).
        ///
        /// Devuelve true si la notificacion fue creada.
        /// </summary>
        public static async Task<bool> EnviarNotificacionAsync(
            AppDbContext context,
            EmailService emailService,
            string idUsuario,
            string categoria,
            string tipo,
            string titulo,
            string mensaje)
        {
            var preferencia = await context.PreferenciasNotificacion
                .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario);

            // Si el alumno desactivo esta categoria, no se envia nada
            if (preferencia != null && !CategoriaHabilitada(preferencia, categoria))
            {
                return false;
            }

            // Evita duplicados: misma notificacion para el mismo usuario en los ultimos 2 minutos
            var yaExiste = await context.Notificaciones.AnyAsync(n =>
                n.IdUsuario == idUsuario &&
                n.Tipo == tipo &&
                n.Titulo == titulo &&
                n.Mensaje == mensaje &&
                n.FechaEnvio >= DateTime.Now.AddMinutes(-2));

            if (yaExiste)
            {
                return false;
            }

            var notificacion = new Notificacion
            {
                IdUsuario = idUsuario,
                Tipo = tipo,
                Titulo = titulo,
                Mensaje = mensaje,
                Leida = false,
                FechaEnvio = DateTime.Now,
                CanalUsado = "Plataforma",
                EnvioFallido = false
            };

            // USER-09-008: si el canal preferido es Email, se intenta enviar tambien un correo real.
            var canal = preferencia?.CanalPreferido ?? "Push";

            if (canal == "Email")
            {
                var usuario = await context.Users.FirstOrDefaultAsync(u => u.Id == idUsuario);

                if (usuario == null || string.IsNullOrWhiteSpace(usuario.Email))
                {
                    // El canal seleccionado no esta disponible (no hay correo registrado):
                    // se deja el aviso por el medio alterno (plataforma) y se registra el error.
                    notificacion.CanalUsado = "Plataforma";
                    notificacion.EnvioFallido = true;
                }
                else
                {
                    try
                    {
                        await emailService.EnviarCorreoAsync(usuario.Email, titulo, mensaje);
                        notificacion.CanalUsado = "Email";
                        notificacion.EnvioFallido = false;
                    }
                    catch
                    {
                        // El canal preferido (correo) no esta disponible en este momento
                        // (SMTP caido, credenciales invalidas, etc.). Se intenta el otro
                        // medio: la notificacion queda disponible dentro de la app, y se
                        // registra el error para que quede constancia de la falla.
                        notificacion.CanalUsado = "Plataforma";
                        notificacion.EnvioFallido = true;
                    }
                }
            }

            // La notificacion siempre queda guardada en la app (canal "Plataforma" por defecto)
            context.Notificaciones.Add(notificacion);

            return true;
        }

        private static bool CategoriaHabilitada(PreferenciaNotificacion preferencia, string categoria)
        {
            return categoria switch
            {
                "Pago" => preferencia.NotificacionesPago,
                "Clase" => preferencia.NotificacionesClase,
                "Recordatorio" => preferencia.NotificacionesRecordatorio,
                "Campeonato" => preferencia.NotificacionesCampeonato,
                _ => true
            };
        }
    }
}
