using System.ComponentModel.DataAnnotations;

namespace Control_De_Tareas.Models
{
    public class TareasVm
    {
        public Guid Id { get; set; }
        [Required (ErrorMessage ="El titulo es requirido")]
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public Guid CourseId { get; set; }
        public decimal MaxScore { get; set; }
    }
}
