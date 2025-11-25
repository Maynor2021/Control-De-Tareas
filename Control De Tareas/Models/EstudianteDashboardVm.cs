// Ruta: Control_De_Tareas\Models\EstudianteDashboardVm.cs
using System;
using System.Collections.Generic;

namespace Control_De_Tareas.Models
{
    public class EstudianteDashboardVm
    {
        public string NombreEstudiante { get; set; } = "Estudiante";

        public List<CursoInscritoVm> CursosInscritos { get; set; } = new List<CursoInscritoVm>();

        // Tareas pendientes (listas simples)
        public List<EntregaPendienteVm> TareasPendientes { get; set; } = new List<EntregaPendienteVm>();

        // Últimas calificaciones
        public List<CalificacionVm> UltimasCalificaciones { get; set; } = new List<CalificacionVm>();

        // Promedio general (nullable)
        public decimal? PromedioGeneral { get; set; }

        // Tareas por curso (para el accordion)
        public List<TareasPorCursoVm> PendientesPorCurso { get; set; } = new List<TareasPorCursoVm>();

        // Próximas entregas (puede reutilizar TareaVm o un DTO específico)
        public List<TareaVm> ProximasEntregas { get; set; } = new List<TareaVm>();
    }
}
