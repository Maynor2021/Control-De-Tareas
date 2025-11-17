using System.ComponentModel.DataAnnotations.Schema;

namespace Control_De_Tareas.Data.Entitys
{
    [Table("Roles")]
    public class Roles
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSoftDeleted { get; set; }
        public ICollection<UserRoles> UserRoles { get; set; } = new List<UserRoles>();
    }
}