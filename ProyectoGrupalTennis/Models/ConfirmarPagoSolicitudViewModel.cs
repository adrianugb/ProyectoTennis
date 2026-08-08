namespace ProyectoGrupalTennis.Models
{
    public class ConfirmarPagoSolicitudViewModel
    {
        public int IdSolicitudCurso { get; set; }

        public string Concepto { get; set; }
            = string.Empty;

        public decimal Monto { get; set; }

        public bool EsADomicilio { get; set; }

        public string? DireccionDomicilio { get; set; }

        public DateTime? FechaPropuesta { get; set; }

        public TimeSpan? HoraInicioPropuesta { get; set; }

        public TimeSpan? HoraFinPropuesta { get; set; }

        public bool TieneUbicacion { get; set; }

        public string? NombreZona { get; set; }

        public string? DireccionUbicacion { get; set; }

        public decimal DistanciaKm { get; set; }

        public decimal CostoFijoZona { get; set; }

        public decimal TarifaPorKm { get; set; }

        public decimal CostoPorDistancia { get; set; }

        public decimal CostoDesplazamiento { get; set; }

        public decimal MontoTotal { get; set; }

        public string Profesor { get; set; }
            = string.Empty;

        public string? Cancha { get; set; }
    }
}