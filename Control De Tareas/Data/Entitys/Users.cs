namespace Control_De_Tareas.Data.Entitys
{
    public class Users
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Instructor { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreateAt { get; set; }
        public bool IsSoftDeleted { get; set; }

        // Navigation properties
        public ICollection<UserRoles> UserRoles { get; set; }
    }
}
