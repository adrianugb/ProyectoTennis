using System;
using System.Collections.Generic;

namespace ProyectoGrupalTennis.Models
{
    public class DashboardAdminViewModel
    {
        // Filtro de Fechas (ADM-08-004)
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // KPIs (ADM-08-001 / ADM-08-002 / ADM-08-003)
        public int AlumnosActivos { get; set; }
        public int AlumnosNuevosEnPeriodo { get; set; }

        public int ClasesProgramadas { get; set; }

        public int ProfesoresActivos { get; set; }

        public decimal IngresosPeriodo { get; set; }

        // Tabla de clases del periodo
        public List<ClaseResumenViewModel> ClasesDelPeriodo { get; set; } = new();

        // Alertas rápidas
        public List<string> Alertas { get; set; } = new();

        // ADM-08-001: cursos con mas matriculas en el periodo
        public List<CursoDemandaViewModel> CursosMasDemandados { get; set; } = new();
    }

    public class ClaseResumenViewModel
    {
        public string NombreCurso { get; set; }
        public string Profesor { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public int CuposOcupados { get; set; }
        public int CuposTotales { get; set; }
        public string Estado { get; set; }
    }

    public class CursoDemandaViewModel
    {
        public string NombreCurso { get; set; }
        public int TotalMatriculas { get; set; }
    }
}