namespace Control_De_Tareas.Data.Entitys
{
    /// <summary>
    /// Representa un rol dentro del sistema.
    /// Cada usuario debe tener uno o varios roles para determinar
    /// sus permisos y accesos a las funcionalidades del sistema.
    /// </summary>
    public class Roles
    {
        /// <summary>
        /// Identificador único del rol.
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Nombre del rol (ejemplo: "admin", "profesor", "estudiante").
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Descripción breve del rol y sus responsabilidades.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Fecha en que se registró el rol dentro del sistema.
        /// </summary>
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// Indica si el rol está eliminado lógicamente (soft delete).
        /// </summary>
        public Boolean IsSoftDeleted { get; set; }

        /// <summary>
        /// Lista de relaciones entre usuarios y este rol.
        /// Representa la asignación del rol a distintos usuarios.
        /// </summary>
        public ICollection<UserRoles> UserRoles { get; set; }
    }
}
