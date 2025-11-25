using Control_De_Tareas.Data.Entitys;
using Microsoft.EntityFrameworkCore;
using Control_De_Tareas.Data.Configurations;

namespace Control_De_Tareas.Data
{
    public class ContextDB : DbContext
    {
        public ContextDB(DbContextOptions<ContextDB> options) : base(options)
        {

  // ========== DbSets (Tablas) ==========
        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Tareas> Tareas { get; set; }
        public DbSet<Submissions> Submissions { get; set; }
        public DbSet<Module> Module { get; set; }
        public DbSet<ModuleGroup> ModuleGroup { get; set; }
        public DbSet<RoleModules> RoleModules { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<CourseOfferings> CourseOfferings { get; set; }
        public DbSet<Enrollments> Enrollments { get; set; }
        public DbSet<Announcements> Announcements { get; set; }
        public DbSet<Periods> Periods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== CONFIGURACIONES ==========
            modelBuilder.ApplyConfiguration(new UsersConfig());
            modelBuilder.ApplyConfiguration(new RolesConfig());
            modelBuilder.ApplyConfiguration(new UserRolesConfig());
            modelBuilder.ApplyConfiguration(new CoursesConfiguration());
            modelBuilder.ApplyConfiguration(new TareasConfig());
            modelBuilder.ApplyConfiguration(new SubmissionsConfig());
            modelBuilder.ApplyConfiguration(new ModuleGroupConfig());
            modelBuilder.ApplyConfiguration(new RoleModulesConfig());
            modelBuilder.ApplyConfiguration(new ModuleConfig());
            modelBuilder.ApplyConfiguration(new EnrollmentsConfig());
            modelBuilder.ApplyConfiguration(new AnnouncementsConfig());
            modelBuilder.ApplyConfiguration(new AuditLogsConfig());
            modelBuilder.ApplyConfiguration(new GradesConfig());
            modelBuilder.ApplyConfiguration(new SubmissionFilesConfig());
            modelBuilder.ApplyConfiguration(new CourseOfferingsConfig());
            modelBuilder.ApplyConfiguration(new PeriodsConfig());

            // ========== RELACIONES ==========

            // 1. CourseOffering -> Course (muchos a uno)
            modelBuilder.Entity<CourseOfferings>()
                .HasOne(co => co.Course)
                .WithMany()
                .HasForeignKey(co => co.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. CourseOffering -> Period (muchos a uno)
            modelBuilder.Entity<CourseOfferings>()
                .HasOne(co => co.Period)
                .WithMany(p => p.CourseOfferings)
                .HasForeignKey(co => co.PeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. CourseOffering -> Professor (Users) (muchos a uno)
            modelBuilder.Entity<CourseOfferings>()
                .HasOne(co => co.Professor)
                .WithMany()
                .HasForeignKey(co => co.ProfessorId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Enrollment -> CourseOffering (muchos a uno)
            modelBuilder.Entity<Enrollments>()
                .HasOne(e => e.CourseOffering)
                .WithMany(co => co.Enrollments)
                .HasForeignKey(e => e.CourseOfferingId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Enrollment -> Student (Users) (muchos a uno)
            modelBuilder.Entity<Enrollments>()
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Tarea -> CourseOffering (muchos a uno)
            modelBuilder.Entity<Tareas>()
                .HasOne(t => t.CourseOffering)
                .WithMany(co => co.Tareas)
                .HasForeignKey(t => t.CourseOfferingId)
                .OnDelete(DeleteBehavior.Restrict);

            // 7. Announcement -> CourseOffering (muchos a uno)
            modelBuilder.Entity<Announcements>()
                .HasOne(a => a.CourseOffering)
                .WithMany(co => co.Announcements)
                .HasForeignKey(a => a.CourseOfferingId)
                .OnDelete(DeleteBehavior.Restrict);

            // 8. Índice único para CourseOffering
            modelBuilder.Entity<CourseOfferings>()
                .HasIndex(co => new { co.CourseId, co.PeriodId, co.Section })
                .IsUnique()
                .HasFilter("[IsSoftDeleted] = 0");
        }
    }
}