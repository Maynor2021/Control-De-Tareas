using System.ComponentModel.DataAnnotations;

namespace Control_De_Tareas.Data.ViewModels
{
    public class TareaCreateVm
    {
        // Relación con CourseOfferings (INT según tu BD)
        public int CourseOfferingId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "La fecha límite es obligatoria.")]
        public DateTime DueDate { get; set; }

        [Range(0, 10000, ErrorMessage = "Los puntos deben estar entre 0 y 10000.")]
        public decimal MaxScore { get; set; } = 100;
    }
}

