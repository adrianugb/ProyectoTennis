using AcademiaTennisDAL.Context;
using AcademiaTennisDAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGrupalTennis.Helpers
{
    // USER-09-009: punto unico para crear notificaciones.
    // Respeta las preferencias del alumno y evita notificaciones duplicadas.
    public static class NotificacionHelper
    {
        /// <summary>
        /// Crea una notificacion para el usuario, siempre y cuando:
        /// 1) el alumno no haya desactivado esa categoria de aviso, y
        /// 2) no exista ya una notificacion identica muy reciente (evita duplicados).
        /// Categorias validas: "Pago", "Clase", "Recordatorio", "Campeonato".
        /// Devuelve true si la notificacion fue agregada al contexto (falta SaveChangesAsync).
        /// </summary>
        public static async Task<bool> EnviarNotificacionAsync(
            AppDbContext context,
            string idUsuario,
            string categoria,
            string tipo,
            string titulo,
            string mensaje)
        {
            var preferencia = await context.PreferenciasNotificacion
                .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario);

            // Si el alumno tiene preferencias guardadas y desactivo esta categoria, no se envia
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

            context.Notificaciones.Add(new Notificacion
            {
                IdUsuario = idUsuario,
                Tipo = tipo,
                Titulo = titulo,
                Mensaje = mensaje,
                Leida = false,
                FechaEnvio = DateTime.Now
            });

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