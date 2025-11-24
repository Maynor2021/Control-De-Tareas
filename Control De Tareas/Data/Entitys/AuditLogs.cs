using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
    [Table("AuditLogs")]
    public class AuditLogs
    {
        public long Id { get; set; }
        public Guid? UserId { get; set; }
        public string? Action { get; set; }
        public string? Entity { get; set; }
        public int? EntityId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? Details { get; set; }
        public bool IsSoftDeleted { get; set; }
        public Users? User { get; set; }
    }
}