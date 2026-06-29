namespace ProyectoGrupalTennis.Models
{
    public class AdminHistorialPagosViewModel
    {

        public string? FiltroBuscar { get; set; }

        public string? FiltroEstado { get; set; }

        public string? FiltroFactura { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }
        public List<AdminPagoItemViewModel> Pagos { get; set; } = new();
    }

    public class AdminPagoItemViewModel
    {
        public int IdPago { get; set; }

        public string Alumno { get; set; } = string.Empty;

        public string Concepto { get; set; } = string.Empty;

        public string MetodoPago { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string FacturaEstado { get; set; } = string.Empty;

        public DateTime? FechaFactura { get; set; }
    }
}