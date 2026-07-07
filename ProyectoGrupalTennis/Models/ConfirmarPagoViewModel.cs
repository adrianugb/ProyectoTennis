namespace ProyectoGrupalTennis.Models
{
    public class ConfirmarPagoViewModel
    {
        public int IdCurso { get; set; }

        public int? IdReserva { get; set; }
        public string Concepto { get; set; } = string.Empty;

        public decimal Monto { get; set; }

    }
}