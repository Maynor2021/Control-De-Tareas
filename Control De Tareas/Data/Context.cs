using Control_De_Tareas.Data.Entitys;
using Microsoft.EntityFrameworkCore;

namespace Control_De_Tareas.Data
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
        public DbSet<Periods> Periods { get; set; }
        public DbSet<CourseOfferings> CourseOfferings { get; set; }
        public DbSet<Enrollments> Enrollments { get; set; }
        public DbSet<SubmissionFiles> SubmissionFiles { get; set; }
        public DbSet<Grades> Grades { get; set; }
        public DbSet<Announcements> Announcements { get; set; }
        public DbSet<AuditLogs> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new Configurations.UsersConfig());
            modelBuilder.ApplyConfiguration(new Configurations.RolesConfig());
            modelBuilder.ApplyConfiguration(new Configurations.UserRolesConfig());
            modelBuilder.ApplyConfiguration(new Configurations.CoursesConfig());
            modelBuilder.ApplyConfiguration(new Configurations.TareasConfig());
            modelBuilder.ApplyConfiguration(new Configurations.SubmissionsConfig());
            modelBuilder.ApplyConfiguration(new Configurations.PeriodsConfig());
            modelBuilder.ApplyConfiguration(new Configurations.CourseOfferingsConfig());
            modelBuilder.ApplyConfiguration(new Configurations.EnrollmentsConfig());
            modelBuilder.ApplyConfiguration(new Configurations.SubmissionFilesConfig());
            modelBuilder.ApplyConfiguration(new Configurations.GradesConfig());
            modelBuilder.ApplyConfiguration(new Configurations.AnnouncementsConfig());
            modelBuilder.ApplyConfiguration(new Configurations.AuditLogsConfig());
        }
    }
}