using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
    [Table("CourseOfferings")]
    public class CourseOfferings
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int ProfessorId { get; set; }
        public int PeriodId { get; set; }
        public string? Section { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public bool IsSoftDeleted { get; set; }
        public Courses Course { get; set; } = null!;
        public Users Professor { get; set; } = null!;
        public Periods Period { get; set; } = null!;
        public ICollection<Enrollments> Enrollments { get; set; } = new List<Enrollments>();
        public ICollection<Tareas> Tareas { get; set; } = new List<Tareas>();
        public ICollection<Announcements> Announcements { get; set; } = new List<Announcements>();
    }
}