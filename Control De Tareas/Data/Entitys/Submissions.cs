using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
    [Table("Submissions")]
    public class Submissions
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int StudentId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string? Comments { get; set; }
        public decimal? CurrentGrade { get; set; }
        public string Status { get; set; } = "Submitted";
        public bool IsSoftDeleted { get; set; }
        public Tareas Task { get; set; } = null!;
        public Users Student { get; set; } = null!;
        public ICollection<SubmissionFiles> SubmissionFiles { get; set; } = new List<SubmissionFiles>();
        public ICollection<Grades> Grades { get; set; } = new List<Grades>();
    }
}