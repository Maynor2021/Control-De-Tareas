using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
<<<<<<< HEAD
    [Table("UserRoles")]
    public class UserRoles
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsSoftDeleted { get; set; }
        public Users User { get; set; } = null!;
        public Roles Role { get; set; } = null!;
=======
    /// <summary>
    /// Representa la relación entre un usuario y un rol dentro del sistema.
    /// Es la tabla intermedia de la relación muchos-a-muchos entre Users y Roles.
    /// </summary>
    public class UserRoles
    {
        /// <summary>
        /// Identificador único del registro UserRole.
        /// </summary>
        public Guid UserRoleId { get; set; }

        /// <summary>
        /// Identificador del usuario al que pertenece este rol.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Identificador del rol asignado al usuario.
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Fecha y hora en que el rol fue asignado al usuario.
        /// </summary>
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// Indica si el registro está eliminado lógicamente (soft delete).
        /// </summary>
        public bool IsSoftDeleted { get; set; }

        // Navigation properties

        /// <summary>
        /// Referencia al usuario al que pertenece esta asignación.
        /// </summary>
        public Users User { get; set; }

        /// <summary>
        /// Referencia al rol asignado.
        /// </summary>
        public Roles Role { get; set; }
>>>>>>> 8ae78bba429b216c9e17d1cad0ac8c857ba79d25
    }
}