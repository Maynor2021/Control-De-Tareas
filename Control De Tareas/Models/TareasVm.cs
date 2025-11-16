using System.ComponentModel.DataAnnotations;

namespace Control_De_Tareas.Models
{
    /// <summary>
    /// ViewModel utilizado para representar los datos de una tarea
    /// dentro del sistema. Este modelo es usado en vistas para crear,
    /// editar o visualizar tareas asignadas a un curso.
    /// </summary>
    public class TareasVm
    {
        /// <summary>
        /// Identificador único de la tarea.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Título de la tarea. Campo obligatorio.
        /// </summary>
        [Required(ErrorMessage = "El título es requerido")]
        public string Title { get; set; }

        /// <summary>
        /// Descripción detallada de la tarea.
        /// Explica al estudiante qué debe entregar.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Fecha límite de entrega de la tarea.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Identificador del curso al que pertenece esta tarea.
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// Puntuación máxima que el estudiante puede obtener al completar la tarea.
        /// </summary>
        public decimal MaxScore { get; set; }
    }
}
