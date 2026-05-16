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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}