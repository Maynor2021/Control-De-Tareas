namespace Control_De_Tareas.Data.Entitys
{
    /// <summary>
    /// Representa una entrega realizada por un estudiante para una tarea específica.
    /// Contiene información como fecha de entrega, calificación actual y comentarios del estudiante o profesor.
    /// </summary>
    public class Submissions
    {
        /// <summary>
        /// Identificador único de la entrega.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador de la tarea a la que corresponde esta entrega.
        /// </summary>
        public Guid TareaId { get; set; }

        /// <summary>
        /// Identificador del estudiante que realizó la entrega.
        /// </summary>
        public Guid StudentId { get; set; }

        /// <summary>
        /// Fecha y hora en que el estudiante entregó la tarea.
        /// </summary>
        public DateTime SubmittedAt { get; set; }

        /// <summary>
        /// Calificación actual asignada a esta entrega.
        /// Puede actualizarse posteriormente por el profesor.
        /// </summary>
        public decimal CurrentGrade { get; set; }

        /// <summary>
        /// Comentarios adicionales sobre la entrega.
        /// Puede ser ingresado por el estudiante o el profesor.
        /// </summary>
        public string? Comments { get; set; }

        // ========================
        // Navigation properties
        // ========================

        /// <summary>
        /// Referencia a la tarea a la que corresponde esta entrega.
        /// </summary>
        public Tareas Tarea { get; set; }

        /// <summary>
        /// Información del estudiante que realizó la entrega.
        /// </summary>
        public Users Student { get; set; }
    }
}
