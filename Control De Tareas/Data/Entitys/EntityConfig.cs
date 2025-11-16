using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Control_De_Tareas.Data.Entitys;

namespace Control_De_Tareas.Data.Configurations
{
    /// <summary>
    /// Configuración de la entidad Tareas para EF Core.
    /// Define validaciones, longitudes, relaciones y reglas de borrado.
    /// </summary>
    public class TareasConfig : IEntityTypeConfiguration<Tareas>
    {
        /// <summary>
        /// Aplica la configuración detallada de la entidad Tareas al modelo.
        /// </summary>
        public void Configure(EntityTypeBuilder<Tareas> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(t => t.Description)
                   .HasMaxLength(1000);

            builder.Property(t => t.DueDate)
                   .IsRequired();

            builder.Property(p => p.MaxScore)
                   .IsRequired()
                   .HasPrecision(18, 2);

            builder.HasOne(t => t.Course)
                   .WithMany()
                   .HasForeignKey(t => t.CourseId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.CreatedByUser)
                   .WithMany()
                   .HasForeignKey(t => t.CreatedBy)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    /// <summary>
    /// Configuración de la entidad Courses.
    /// Aplica restricciones, relaciones y valores por defecto.
    /// </summary>
    public class CoursesConfig : IEntityTypeConfiguration<Courses>
    {
        /// <summary>
        /// Aplica reglas de base de datos para la entidad Courses.
        /// </summary>
        public void Configure(EntityTypeBuilder<Courses> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Codigo)
                   .IsRequired()
                   .HasMaxLength(9);

            builder.Property(p => p.Nombre)
                   .IsRequired()
                   .HasMaxLength(9);

            builder.Property(p => p.Descripcion)
                   .IsRequired(false)
                   .HasMaxLength(500);

            builder.Property(p => p.Estado)
                   .IsRequired()
                   .HasMaxLength(20)
                   .HasDefaultValue("Activo");

            builder.Property(c => c.FechaCreacion)
                   .IsRequired()
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(c => c.Instructor)
                   .WithMany()
                   .HasForeignKey(c => c.InstructorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Tareas)
                   .WithOne(t => t.Course)
                   .HasForeignKey(t => t.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// Configuración de la entidad Roles.
    /// Define longitudes, restricciones y relación con UserRoles.
    /// </summary>
    public class RolesConfig : IEntityTypeConfiguration<Roles>
    {
        /// <summary>
        /// Aplica la configuración para EF Core de la entidad Roles.
        /// </summary>
        public void Configure(EntityTypeBuilder<Roles> builder)
        {
            builder.HasKey(r => r.RoleId);

            builder.Property(r => r.RoleName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(r => r.Description)
                   .HasMaxLength(200)
                   .IsRequired(false);

            builder.Property(r => r.CreateAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETDATE()");

            builder.HasMany(r => r.UserRoles)
                   .WithOne(ur => ur.Role)
                   .HasForeignKey(ur => ur.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// Configuración de la entidad Users.
    /// Define restricciones de formato, índices únicos y relaciones.
    /// </summary>
    public class UsersConfig : IEntityTypeConfiguration<Users>
    {
        /// <summary>
        /// Configura la entidad Users para EF Core.
        /// </summary>
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            builder.HasKey(u => u.UserId);

            builder.Property(u => u.UserName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasIndex(u => u.Email)
                   .IsUnique();

            builder.Property(u => u.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(u => u.Instructor)
                   .HasMaxLength(100);

            builder.Property(u => u.CreateAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETDATE()");

            builder.HasMany(u => u.UserRoles)
                   .WithOne(ur => ur.User)
                   .HasForeignKey(ur => ur.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// Configuración de la tabla intermedia UserRoles.
    /// Define índices, relaciones y restricciones.
    /// </summary>
    public class UserRolesConfig : IEntityTypeConfiguration<UserRoles>
    {
        /// <summary>
        /// Configura la entidad UserRoles para EF Core.
        /// </summary>
        public void Configure(EntityTypeBuilder<UserRoles> builder)
        {
            builder.HasKey(ur => ur.UserRoleId);

            builder.Property(ur => ur.CreateAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(ur => ur.User)
                   .WithMany(u => u.UserRoles)
                   .HasForeignKey(ur => ur.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ur => ur.Role)
                   .WithMany(r => r.UserRoles)
                   .HasForeignKey(ur => ur.RoleId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ur => new { ur.UserId, ur.RoleId });
        }
    }

    /// <summary>
    /// Configuración de la entidad Submissions.
    /// Define validaciones, relaciones y restricciones de calificación.
    /// </summary>
    public class SubmissionsConfig : IEntityTypeConfiguration<Submissions>
    {
        /// <summary>
        /// Aplica al modelo la configuración detallada de Submissions.
        /// </summary>
        public void Configure(EntityTypeBuilder<Submissions> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.TareaId)
                   .IsRequired();

            builder.Property(s => s.StudentId)
                   .IsRequired();

            builder.Property(s => s.SubmittedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(s => s.CurrentGrade)
                   .HasColumnType("decimal(5,2)")
                   .IsRequired();

            builder.Property(s => s.Comments)
                   .HasMaxLength(1000)
                   .IsRequired(false);

            builder.HasOne(s => s.Tarea)
                   .WithMany(t => t.Submissions)
                   .HasForeignKey(s => s.TareaId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Student)
                   .WithMany()
                   .HasForeignKey(s => s.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => s.TareaId);
            builder.HasIndex(s => s.StudentId);
            builder.HasIndex(s => s.SubmittedAt);
        }
    }
}
