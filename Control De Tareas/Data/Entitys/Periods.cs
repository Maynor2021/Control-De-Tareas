using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
    [Table("Periods")]
    public class Periods
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsSoftDeleted { get; set; }
        public ICollection<CourseOfferings> CourseOfferings { get; set; } = new List<CourseOfferings>();
    }
}