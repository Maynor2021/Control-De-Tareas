using System.ComponentModel.DataAnnotations;

namespace Control_De_Tareas.Models
{
    public class RolVm
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public DateTime CreateAt { get; set; }
        public bool IsSoftDeleted { get; set; }
    }
}