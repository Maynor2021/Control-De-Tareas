namespace Control_De_Tareas.Models
{
    public class CursoVm
    {
        public Guid Id { get; set; } // CAMBIADO de int a Guid
        public string Codigo { get; set; }
        public string Titulo { get; set; }
        public string Seccion { get; set; }
    }
}