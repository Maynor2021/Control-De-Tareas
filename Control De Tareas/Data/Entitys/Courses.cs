using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
<<<<<<< HEAD
    [Table("Courses")]
    public class Courses
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public bool IsSoftDeleted { get; set; }
=======
    /// <summary>
    /// Representa un curso dentro de la plataforma.
    /// Contiene información general como código, nombre, descripción,
    /// instructor asignado y tareas relacionadas.
    /// </summary>
    public class Courses
    {
        /// <summary>
        /// Identificador único del curso.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Código del curso (por ejemplo: MAT101).
        /// </summary>
        public string Codigo { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del curso (por ejemplo: Matemáticas Básicas).
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción opcional del curso.
        /// Puede incluir detalles adicionales sobre el contenido.
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Identificador del instructor asignado al curso.
        /// </summary>
        public Guid InstructorId { get; set; }

        /// <summary>
        /// Estado del curso (Activo, Inactivo, Cancelado).
        /// Valor por defecto: "Activo".
        /// </summary>
        public string Estado { get; set; } = "Activo";

        /// <summary>
        /// Fecha de creación del curso dentro del sistema.
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // ========================
        // Navigation properties
        // ========================

        /// <summary>
        /// Usuario que funge como instructor del curso.
        /// </summary>
        public Users? Instructor { get; set; }

        /// <summary>
        /// Lista de tareas asociadas al curso.
        /// </summary>
        public ICollection<Tareas>? Tareas { get; set; }
>>>>>>> 8ae78bba429b216c9e17d1cad0ac8c857ba79d25
    }
}