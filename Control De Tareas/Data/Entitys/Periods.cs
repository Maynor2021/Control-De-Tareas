using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
     public class Periods
    {
        
        public int Id { get; set; }

      
        public string Name { get; set; }


        public DateTime StartDate { get; set; }


        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        // Propiedad de navegación
        public virtual ICollection<CourseOfferings> CourseOfferings { get; set; }
    }
}