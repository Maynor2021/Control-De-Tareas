using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
<<<<<<< HEAD
    [Table("Tasks")]
    public class Tareas
    {
        public int Id { get; set; }
        public int CourseOfferingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public int CreatedBy { get; set; }
        public decimal MaxScore { get; set; } = 100;
        public bool IsSoftDeleted { get; set; }
        public CourseOfferings CourseOffering { get; set; } = null!;
        public Users CreatedByUser { get; set; } = null!;
        public ICollection<Submissions> Submissions { get; set; } = new List<Submissions>();
=======
    /// <summary>
    /// Representa una tarea asignada dentro de un curso.
    /// Contiene información como título, descripción, fecha límite, puntaje máximo
    /// y quién la creó. Además, mantiene relaciones con cursos, usuarios y entregas.
    /// </summary>
    public class Tareas
    {
        /// <summary>
        /// Identificador único de la tarea.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador del curso al que pertenece esta tarea.
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// Título de la tarea (lo que verá el estudiante).
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Descripción detallada de la tarea.
        /// Indica las instrucciones y criterios de entrega.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Fecha límite para entregar la tarea.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Identificador del usuario (profesor) que creó la tarea.
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Puntaje máximo que puede obtener un estudiante al completar la tarea.
        /// </summary>
        public decimal MaxScore { get; set; }

        /// <summary>
        /// Indica si la tarea está eliminada de forma lógica (soft delete).
        /// </summary>
        public bool IsSoftDeleted { get; set; }

        // ========================
        // Navigation properties
        // ========================

        /// <summary>
        /// Curso al que pertenece la tarea.
        /// </summary>
        public Courses Course { get; set; }

        /// <summary>
        /// Usuario creador de la tarea (generalmente un profesor).
        /// </summary>
        public Users CreatedByUser { get; set; }

        /// <summary>
        /// Lista de entregas realizadas por los estudiantes para esta tarea.
        /// </summary>
        public ICollection<Submissions> Submissions { get; set; }
>>>>>>> 8ae78bba429b216c9e17d1cad0ac8c857ba79d25
    }
}
