namespace Control_De_Tareas.Models
{
    public class CursosVm
    {
        public List<CursoDto> Cursos { get; set; } = new List<CursoDto>();
    }

    public class CursoDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string InstructorNombre { get; set; } = string.Empty;
        public int CantidadEstudiantes { get; set; }
        public string Estado { get; set; } = "Activo";
    }
}
