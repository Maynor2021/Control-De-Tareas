using Microsoft.EntityFrameworkCore;
using Control_De_Tareas.Data.Entitys;
using Control_De_Tareas.Data.Configurations;

namespace Control_De_Tareas.Data.Entitys
{
    public class ContextDB : DbContext
    {
        public ContextDB(DbContextOptions<ContextDB> options) : base(options)
        {

        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Tareas> Tareas { get; set; }
        public DbSet<Submissions> Submissions { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<Module> Module { get; set; }
        public DbSet<ModuleGroup> ModuleGroup { get; set; }
        public DbSet<RoleModules> RoleModules { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<Periods> Periods { get; set; }
        public DbSet<CourseOfferings> CourseOfferings { get; set; }
        public DbSet<Enrollments> Enrollments { get; set; }
        public DbSet<SubmissionFiles> SubmissionFiles { get; set; }
        public DbSet<Grades> Grades { get; set; }
        public DbSet<Announcements> Announcements { get; set; }
        public DbSet<AuditLogs> AuditLogs { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<ModuleGroup> ModuleGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar TODAS las configuraciones
            modelBuilder.ApplyConfiguration(new UsersConfig());
            modelBuilder.ApplyConfiguration(new RolesConfig());
            modelBuilder.ApplyConfiguration(new UserRolesConfig());
            modelBuilder.ApplyConfiguration(new CoursesConfiguration());
            modelBuilder.ApplyConfiguration(new TareasConfig());
            modelBuilder.ApplyConfiguration(new SubmissionsConfig());
            modelBuilder.ApplyConfiguration(new ModuleGroupConfig());
            modelBuilder.ApplyConfiguration(new RoleModulesConfig());
            modelBuilder.ApplyConfiguration(new ModuleConfig());

            modelBuilder.ApplyConfiguration(new AnnouncementsConfig());
            modelBuilder.ApplyConfiguration(new AuditLogsConfig());
            modelBuilder.ApplyConfiguration(new GradesConfig());
            modelBuilder.ApplyConfiguration(new SubmissionFilesConfig());
            modelBuilder.ApplyConfiguration(new CourseOfferingsConfig());
            modelBuilder.ApplyConfiguration(new EnrollmentsConfig());
            modelBuilder.ApplyConfiguration(new PeriodsConfig());
        }
    }
}