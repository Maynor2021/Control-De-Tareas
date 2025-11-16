using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
)]
    public class Enrollments
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseOfferingId { get; set; }

        [Required]
        public int StudentId { get; set; }

        public DateTime EnrolledAt { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        // Propiedades de navegación
        [ForeignKey("CourseOfferingId")]
        public virtual CourseOfferings CourseOffering { get; set; }

        [ForeignKey("StudentId")]
        public virtual Users Student { get; set; }
    }
}
