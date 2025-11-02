namespace Control_De_Tareas.Data.Entitys
{
    public class UserRoles
    {
        public Guid UserRoleId { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public DateTime CreateAt { get; set; }
        public bool IsSoftDeleted { get; set; }

        // Navigation properties
        public Users User { get; set; }
        public Roles Role { get; set; }
    }
}
