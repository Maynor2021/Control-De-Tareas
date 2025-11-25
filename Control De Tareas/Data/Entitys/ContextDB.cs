using Control_De_Tareas.Data.Entitys;
using Microsoft.EntityFrameworkCore;

namespace Control_De_Tareas.Data
{
    public class ContextDB : DbContext
    {
        public ContextDB(DbContextOptions<ContextDB> options) : base(options)
        {
        }

        // ========== DbSets (Tablas) ==========

        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<Periods> Periods { get; set; }
        public DbSet<CourseOfferings> CourseOfferings { get; set; }
        public DbSet<Enrollments> Enrollments { get; set; }
        public DbSet<Tareas> Tareas { get; set; }
        public DbSet<Submissions> Submissions { get; set; }
        public DbSet<SubmissionFiles> SubmissionFiles { get; set; }
        public DbSet<Grades> Grades { get; set; }
        public DbSet<Announcements> Announcements { get; set; }
        public DbSet<AuditLogs> AuditLogs { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<ModuleGroup> ModuleGroups { get; set; }
        public DbSet<RoleModules> RoleModules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new Configurations.UsersConfig());
            modelBuilder.ApplyConfiguration(new Configurations.RolesConfig());
            modelBuilder.ApplyConfiguration(new Configurations.UserRolesConfig());
            modelBuilder.ApplyConfiguration(new Configurations.CoursesConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.TareasConfig());
            modelBuilder.ApplyConfiguration(new Configurations.SubmissionsConfig());
            modelBuilder.ApplyConfiguration(new Configurations.ModuleGroupConfig());
            modelBuilder.ApplyConfiguration(new Configurations.RoleModulesConfig());
            modelBuilder.ApplyConfiguration(new Configurations.ModuleConfig());
            modelBuilder.ApplyConfiguration(new Configurations.EnrollmentsConfig());
            modelBuilder.ApplyConfiguration(new Configurations.AnnouncementsConfig());

            // ========== NUEVAS RELACIONES ==========

            // 1. Relación: CourseOffering -> Course (muchos a uno)
            // Una oferta pertenece a un curso, un curso puede tener muchas ofertas
            modelBuilder.Entity<CourseOfferings>()
                .HasOne(co => co.Course)
                .WithMany() // Course no tiene navegación inversa hacia CourseOfferings
                .HasForeignKey(co => co.CourseId)
                .OnDelete(DeleteBehavior.Restrict); // No eliminar curso si tiene ofertas

            // 2. Relación: CourseOffering -> Period (muchos a uno)
            // Una oferta pertenece a un período, un período puede tener muchas ofertas
            modelBuilder.Entity<CourseOfferings>()
                .HasOne(co => co.Period)
                .WithMany(p => p.CourseOfferings) // Period SÍ tiene navegación inversa
                .HasForeignKey(co => co.PeriodId)
                .OnDelete(DeleteBehavior.Restrict); // No eliminar período si tiene ofertas

            // 3. Relación: CourseOffering -> Professor (Users) (muchos a uno)
            // Una oferta tiene un profesor, un profesor puede tener muchas ofertas
            modelBuilder.Entity<CourseOfferings>()
                .HasOne(co => co.Professor)
                .WithMany() // Users no tiene navegación inversa hacia CourseOfferings
                .HasForeignKey(co => co.ProfessorId)
                .OnDelete(DeleteBehavior.Restrict); // No eliminar usuario si tiene ofertas

            // 4. Relación: Enrollment -> CourseOffering (muchos a uno)
            // Una inscripción pertenece a una oferta, una oferta tiene muchas inscripciones
            modelBuilder.Entity<Enrollments>()
                .HasOne(e => e.CourseOffering)
                .WithMany(co => co.Enrollments) // CourseOffering SÍ tiene navegación inversa
                .HasForeignKey(e => e.CourseOfferingId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Relación: Enrollment -> Student (Users) (muchos a uno)
            modelBuilder.Entity<Enrollments>()
                .HasOne(e => e.Student)
                .WithMany() // Users no tiene navegación inversa
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Relación: Tarea -> CourseOffering (muchos a uno)
            // Una tarea pertenece a una oferta, una oferta tiene muchas tareas
            modelBuilder.Entity<Tareas>()
                .HasOne(t => t.CourseOffering)
                .WithMany(co => co.Tareas) // CourseOffering SÍ tiene navegación inversa
                .HasForeignKey(t => t.CourseOfferingId)
                .OnDelete(DeleteBehavior.Restrict);

            // 7. Relación: Announcement -> CourseOffering (muchos a uno)
            // Un anuncio pertenece a una oferta, una oferta tiene muchos anuncios
            modelBuilder.Entity<Announcements>()
                .HasOne(a => a.CourseOffering)
                .WithMany(co => co.Announcements) // CourseOffering SÍ tiene navegación inversa
                .HasForeignKey(a => a.CourseOfferingId)
                .OnDelete(DeleteBehavior.Restrict);

            // 8. Índices únicos para CourseOffering (no duplicar ofertas)
            // Un curso no puede tener dos ofertas en el mismo período y sección
            modelBuilder.Entity<CourseOfferings>()
                .HasIndex(co => new { co.CourseId, co.PeriodId, co.Section })
                .IsUnique()
                .HasFilter("[IsSoftDeleted] = 0"); // Solo aplicar a registros no eliminados
        }
    }
}