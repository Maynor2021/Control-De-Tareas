using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
    public class CourseOfferings
    {
        
        public int Id { get; set; }

        public int CourseId { get; set; }

  
        public int ProfessorId { get; set; }

        public int PeriodId { get; set; }

        public string Section { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        // Propiedades de navegación
        public virtual Courses Course { get; set; }

        public virtual Users Professor { get; set; }

        public virtual Periods Period { get; set; }

        public virtual ICollection<Enrollments> Enrollments { get; set; }
    }
}