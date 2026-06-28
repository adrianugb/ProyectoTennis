namespace ProyectoGrupalTennis.Models
{
    public class CatalogoCursosViewModel
    {
        public List<CatalogoCursoItemViewModel> Cursos { get; set; } = new();
    }

    public class CatalogoCursoItemViewModel
    {
        public int IdCurso { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public string Detalle { get; set; } = string.Empty;

        public string Imagen { get; set; } = string.Empty;
    }
}