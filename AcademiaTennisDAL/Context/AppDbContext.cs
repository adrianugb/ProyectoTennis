using AcademiaTennisDAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcademiaTennisDAL.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Oferta> Ofertas { get; set; }

        public DbSet<Cupon> Cupones { get; set; }

        public DbSet<CuponUso> CuponUsos { get; set; }

        public DbSet<Curso> Cursos { get; set; }

        public DbSet<Horario> Horarios { get; set; }

        public DbSet<Matricula> Matriculas { get; set; }

        public DbSet<Cancha> Canchas { get; set; }

        public DbSet<Reserva> Reservas { get; set; }

        public DbSet<AsistenciaClase> AsistenciasClase { get; set; }

        public DbSet<Pago> Pagos { get; set; }

        public DbSet<Factura> Facturas { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }

        public DbSet<PreferenciaNotificacion> PreferenciasNotificacion { get; set; }

        public DbSet<ConfiguracionNotificacion> ConfiguracionesNotificacion { get; set; }

        // Módulo 6 - Asistente Virtual
        public DbSet<PreguntaFrecuente> PreguntasFrecuentes { get; set; }
        public DbSet<ConsultaChatbot> ConsultasChatbot { get; set; }

        // Módulo 10 - Geolocalización
        public DbSet<ZonaCobertura> ZonasCobertura { get; set; }
        public DbSet<UbicacionAlumno> UbicacionesAlumno { get; set; }

        // Módulo 11 - Tienda e Inventario
        public DbSet<Producto> Productos { get; set; }
        public DbSet<TransaccionProducto> TransaccionesProducto { get; set; }

        // Módulo 12 - Campeonatos
        public DbSet<Campeonato> Campeonatos { get; set; }
        public DbSet<InscripcionCampeonato> InscripcionesCampeonato { get; set; }
        public DbSet<Enfrentamiento> Enfrentamientos { get; set; }

        // Módulo 13 - Retención
        public DbSet<AlertaAbandono> AlertasAbandono { get; set; }

        // Módulo 14 - Feedback
        public DbSet<EncuestaSatisfaccion> EncuestasSatisfaccion { get; set; }

        // Módulo 15 - Gamificación
        public DbSet<LogroAlumno> LogrosAlumno { get; set; }
        public DbSet<RachaAsistencia> RachasAsistencia { get; set; }

        // Módulo 16 - Player Card
        public DbSet<ProgresoAlumno> ProgresosAlumno { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Fix: múltiples cascade paths en EncuestasSatisfaccion
            builder.Entity<EncuestaSatisfaccion>()
                .HasOne(e => e.Reserva)
                .WithMany()
                .HasForeignKey(e => e.IdReserva)
                .OnDelete(DeleteBehavior.NoAction);

            // Fix: múltiples cascade paths en Enfrentamiento (3 FKs a AspNetUsers)
            builder.Entity<Enfrentamiento>()
                .HasOne(e => e.Jugador1)
                .WithMany()
                .HasForeignKey(e => e.IdJugador1)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Enfrentamiento>()
                .HasOne(e => e.Jugador2)
                .WithMany()
                .HasForeignKey(e => e.IdJugador2)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Enfrentamiento>()
                .HasOne(e => e.Ganador)
                .WithMany()
                .HasForeignKey(e => e.IdGanador)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}