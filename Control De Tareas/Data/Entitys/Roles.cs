namespace Control_De_Tareas.Data.Entitys
{
    public class Roles
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public DateTime CreateAt { get; set; }
        public Boolean IsSoftDeleted { get; set; }



        public ICollection<UserRoles> UserRoles { get; set; }
    }


}
