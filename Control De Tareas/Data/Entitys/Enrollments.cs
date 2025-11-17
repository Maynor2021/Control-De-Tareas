using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
    [Table("Enrollments")]
    public class Enrollments
    {
        public int Id { get; set; }
        public int CourseOfferingId { get; set; }
        public int StudentId { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active";
        public bool IsSoftDeleted { get; set; }
        public CourseOfferings CourseOffering { get; set; } = null!;
        public Users Student { get; set; } = null!;
    }
}