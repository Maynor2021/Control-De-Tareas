namespace Control_De_Tareas.Models
{
    public class ProfesorDashboardVm
    {
        public string Profesor { get; set; }

        public List<CursoVm> Cursos { get; set; }
        public List<TareaVm> TareasPorCalificar { get; set; }
    }
}

