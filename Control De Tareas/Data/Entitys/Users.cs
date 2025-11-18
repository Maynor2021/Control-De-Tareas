<<<<<<< HEAD
﻿using Microsoft.Identity.Client;
=======
﻿using System.ComponentModel.DataAnnotations.Schema;
>>>>>>> 33251b12da292b3cd7aa9f4be08621805c2e0e30

namespace Control_De_Tareas.Data.Entitys
{
    [Table("Users")]
    public class Users
    {
<<<<<<< HEAD
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Instructor { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreateAt { get; set; }
        public Guid CreatBy { get; set; }
        public Guid ModifieBy { get; set; }
        public bool IsSoftDeleted { get; set; }
         

        //llave foranea 
        public Guid RolId { get; set; }
        public Roles Rol { get; set; }

        // Navigation properties
        public ICollection<UserRoles> UserRoles { get; set; }

        public Users()
        {
            UserRoles = new HashSet<UserRoles>();
        }
=======
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsSoftDeleted { get; set; }
        public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
        public ICollection<CourseOfferings> CourseOfferings { get; set; } = new List<CourseOfferings>();
        public ICollection<Enrollments> Enrollments { get; set; } = new List<Enrollments>();
        public ICollection<Tareas> CreatedTasks { get; set; } = new List<Tareas>();
        public ICollection<Submissions> Submissions { get; set; } = new List<Submissions>();
        public ICollection<Grades> Grades { get; set; } = new List<Grades>();
        public ICollection<Announcements> Announcements { get; set; } = new List<Announcements>();
>>>>>>> 33251b12da292b3cd7aa9f4be08621805c2e0e30
    }
}