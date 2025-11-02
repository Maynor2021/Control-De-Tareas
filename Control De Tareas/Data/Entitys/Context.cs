using Microsoft.EntityFrameworkCore;
using Control_De_Tareas.Data.Entitys;

namespace Control_De_Tareas.Data.Entitys
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<Tareas> Tareas { get; set; }
        public DbSet<Submissions> Submissions { get; set; }
        public DbSet<Courses> Courses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new Configurations.UsersConfig());
            modelBuilder.ApplyConfiguration(new Configurations.RolesConfig());
            modelBuilder.ApplyConfiguration(new Configurations.UserRolesConfig());
            modelBuilder.ApplyConfiguration(new Configurations.CoursesConfig());
            modelBuilder.ApplyConfiguration(new Configurations.TareasConfig());
            modelBuilder.ApplyConfiguration(new Configurations.SubmissionsConfig());
        }

    }
}
