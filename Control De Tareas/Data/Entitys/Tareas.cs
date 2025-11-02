namespace Control_De_Tareas.Data.Entitys
{
    public class Tareas
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public Guid CreatedBy { get; set; }
        public decimal MaxScore { get; set; }
        public bool IsSoftDeleted { get; set; }

        // Navegación
        public Courses Course { get; set; }
        public Users CreatedByUser { get; set; }
        public ICollection<Submissions> Submissions { get; set; }
    }
}