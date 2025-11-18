using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
    /// <summary>
    /// Representa un curso base dentro del sistema.
    /// Un curso puede ser ofertado múltiples veces a través de CourseOfferings.
    /// </summary>
    [Table("Courses")]
    public class Courses
    {
        /// <summary>
        /// Identificador único del curso.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Código del curso (ejemplo: MAT101, PROG202).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Título o nombre del curso.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Descripción opcional del curso.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Fecha en la que el curso fue creado en el sistema.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Indica si el curso se encuentra activo y disponible.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Marca para indicar si el curso fue eliminado de forma lógica (soft delete).
        /// </summary>
        public bool IsSoftDeleted { get; set; }
    }
}
