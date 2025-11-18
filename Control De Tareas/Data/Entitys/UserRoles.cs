using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
    [Table("UserRoles")]
    public class UserRoles
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsSoftDeleted { get; set; }
        public Users User { get; set; } = null!;
        public Roles Role { get; set; } = null!;
    }
}