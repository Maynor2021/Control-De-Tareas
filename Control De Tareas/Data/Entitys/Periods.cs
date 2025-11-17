<<<<<<< HEAD
﻿using System.ComponentModel.DataAnnotations;
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
=======
﻿using System.ComponentModel.DataAnnotations.Schema;

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
>>>>>>> 473eed73bf6e8ed954e4138184de29f4670376cd
    }
}