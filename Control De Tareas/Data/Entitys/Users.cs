using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
<<<<<<< HEAD
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
=======
    /// <summary>
    /// Representa un usuario dentro del sistema.
    /// Puede ser administrador, profesor o estudiante según sus roles asignados.
    /// </summary>
    public class Users
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Nombre del usuario que se mostrará en el sistema.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Nombre del instructor (solo aplicable si el usuario es profesor).
        /// </summary>
        public string Instructor { get; set; }

        /// <summary>
        /// Correo electrónico utilizado para iniciar sesión.
        /// Debe ser único.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Contraseña en formato hash.
        /// (Actualmente se guarda texto plano, pendiente de implementar seguridad).
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Fecha y hora en que el usuario fue creado.
        /// </summary>
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// Indica si el usuario ha sido eliminado lógicamente.
        /// (Soft delete: no se elimina físicamente de la base de datos).
        /// </summary>
        public bool IsSoftDeleted { get; set; }

        /// <summary>
        /// Lista de roles asignados al usuario.
        /// Permite determinar permisos dentro del sistema.
        /// </summary>
        public ICollection<UserRoles> UserRoles { get; set; }
>>>>>>> 8ae78bba429b216c9e17d1cad0ac8c857ba79d25
    }
}