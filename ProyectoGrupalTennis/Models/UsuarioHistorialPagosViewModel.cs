namespace ProyectoGrupalTennis.Models
{
    public class UsuarioHistorialPagosViewModel
    {
        public string? FiltroBuscar { get; set; }

        public string? FiltroEstado { get; set; }

        public string? FiltroFactura { get; set; }

        public List<UsuarioPagoItemViewModel> Pagos { get; set; } = new();
    }

    public class UsuarioPagoItemViewModel
    {
        public int IdPago { get; set; }

        public string Concepto { get; set; } = string.Empty;

        public string MetodoPago { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; }

        public DateTime? FechaFactura { get; set; }

        public string? NumeroFactura { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}