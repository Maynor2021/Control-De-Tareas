using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
    [Table("Users")]
    public class Users
    {
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
    }
}