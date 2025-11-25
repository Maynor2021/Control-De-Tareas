namespace Control_De_Tareas.Models
{
    public class AdminDashboardVm
    {
        public int TotalUsuarios { get; set; }
        public int TotalCursos { get; set; }
        public int TotalTareas { get; set; }

        public List<string> UltimasActividades { get; set; }
    }
}
