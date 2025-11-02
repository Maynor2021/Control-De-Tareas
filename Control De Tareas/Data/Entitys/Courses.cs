namespace Control_De_Tareas.Data.Entitys
{
    public class Courses
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public Guid InstructorId { get; set; }
        public string Estado { get; set; } = "Activo";
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

    
        public Users? Instructor { get; set; }
        public ICollection<Tareas>? Tareas { get; set; }
    }
}
