namespace ProyectoGrupalTennis.Models
{
    public class ConfirmarPagoViewModel
    {
        public int IdCurso { get; set; }

        public int? IdReserva { get; set; }

        public string Concepto { get; set; } = string.Empty;

        // Precio base del curso
        public decimal Monto { get; set; }

        // Modalidad seleccionada
        public bool EsADomicilio { get; set; }

        // Ubicación principal del alumno
        public bool TieneUbicacion { get; set; }

        public int? IdUbicacionAlumno { get; set; }

        public string? DireccionCompleta { get; set; }

        public string? NombreZona { get; set; }

        // Datos del desplazamiento
        public decimal DistanciaKm { get; set; }

        public decimal CostoFijoZona { get; set; }

        public decimal TarifaPorKm { get; set; }

        public decimal CostoPorDistancia { get; set; }

        public decimal CostoDesplazamiento { get; set; }

        // Total final
        public decimal MontoTotal { get; set; }
    }
}