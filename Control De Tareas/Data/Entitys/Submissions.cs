namespace Control_De_Tareas.Data.Entitys
{
    public class Submissions
    {
        public Guid Id { get; set; }
        public Guid TareaId { get; set; }
        public Guid StudentId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public decimal CurrentGrade { get; set; }
        public string? Comments { get; set; }

        // Navigation properties
        public Tareas Tarea { get; set; }
        public Users Student { get; set; }
    }
}
