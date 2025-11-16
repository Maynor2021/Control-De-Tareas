namespace Control_De_Tareas.Models
{
    /// <summary>
    /// ViewModel utilizado para mostrar la lista de cursos en las vistas.
    /// Contiene una colección de objetos CursoDto.
    /// </summary>
    public class CursosVm
    {
        /// <summary>
        /// Lista de cursos mostrados en la interfaz.
        /// Cada curso incluye información resumida como nombre, código e instructor.
        /// </summary>
        public List<CursoDto> Cursos { get; set; } = new List<CursoDto>();
    }

    /// <summary>
    /// DTO (Data Transfer Object) que representa la información resumida de un curso.
    /// Usado para enviar datos procesados desde el controlador hacia la vista.
    /// </summary>
    public class CursoDto
    {
        /// <summary>
        /// Identificador único del curso.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Código del curso (ejemplo: MAT101).
        /// </summary>
        public string Codigo { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del curso (ejemplo: Matemáticas I).
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del instructor asignado al curso.
        /// </summary>
        public string InstructorNombre { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad de estudiantes actualmente inscritos.
        /// (Valor aproximado o calculado externamente.)
        /// </summary>
        public int CantidadEstudiantes { get; set; }

        /// <summary>
        /// Estado del curso (Ej: Activo, Inactivo).
        /// </summary>
        public string Estado { get; set; } = "Activo";
    }
}
