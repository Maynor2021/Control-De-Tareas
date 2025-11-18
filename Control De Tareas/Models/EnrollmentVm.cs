using System.ComponentModel.DataAnnotations;

namespace Control_De_Tareas.Models
{
    public class EnrollmentVm
    {
   
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una oferta de curso")]
        public int CourseOfferingId { get; set; }

  
        [Required(ErrorMessage = "Debe seleccionar un estudiante")]
        public int StudentId { get; set; }

        public DateTime EnrolledAt { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        public string Status { get; set; }
    }
}
