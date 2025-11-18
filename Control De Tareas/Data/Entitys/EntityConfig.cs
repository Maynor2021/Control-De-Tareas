using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Control_De_Tareas.Data.Entitys;



namespace Control_De_Tareas.Data.Configurations
{
    public class TareasConfig : IEntityTypeConfiguration<Tareas>
    {
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


            builder.Property(p => p.MaxScore).IsRequired().HasPrecision(18, 2);

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

    public class CoursesConfig : IEntityTypeConfiguration<Courses>
    {
        public void Configure(EntityTypeBuilder<Courses> builder)
        {

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Codigo).IsRequired().HasMaxLength(9);
            builder.Property(p => p.Nombre).IsRequired().HasMaxLength(9);
            builder.Property(p => p.Descripcion).IsRequired(false).HasMaxLength(500);
            builder.Property(p => p.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Activo");
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

    public class RolesConfig : IEntityTypeConfiguration<Roles>
    {
        public void Configure(EntityTypeBuilder<Roles> builder)
        {

            builder.HasKey(r => r.RoleId);

            // PROPIEDADES
            builder.Property(r => r.RoleName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(r => r.Description)
                   .HasMaxLength(200)
                   .IsRequired(false);

            builder.Property(r => r.CreateAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETDATE()");

            // RELACIONES
            builder.HasMany(r => r.RoleModules)
                   .WithOne(rm => rm.Role)
                   .HasForeignKey(rm => rm.RoleId);

            builder.HasMany(r => r.Users)
                   .WithOne(u => u.Rol)
                   .HasForeignKey(u => u.RolId);

        }
    }

    public class UsersConfig : IEntityTypeConfiguration<Users>
    {
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

    public class UserRolesConfig : IEntityTypeConfiguration<UserRoles>
    {
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

    public class SubmissionsConfig : IEntityTypeConfiguration<Submissions>
    {
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

    public class ModuleGroupConfig : IEntityTypeConfiguration<ModuleGroup>
    {
        public void Configure(EntityTypeBuilder<ModuleGroup> builder)
        {
            builder.HasKey(mg => mg.GroupModuleId);
            builder.Property(mg => mg.Description).IsRequired().HasMaxLength(200);
            builder.Property(mg => mg.CreateAt).IsRequired().HasDefaultValueSql("GETDATE()");
            builder.HasMany(mg => mg.Modules).WithOne(m => m.ModuloAgrupado).HasForeignKey(m => m.ModuloAgrupadoId).OnDelete(DeleteBehavior.Cascade);
        }
    }


    public class ModuleConfig : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.HasKey(m => m.ModuleId);
            builder.HasMany(m => m.RoleModules).WithOne(rm => rm.Module).HasForeignKey(rm => rm.ModuleId);
        }
    }

    public class RoleModulesConfig : IEntityTypeConfiguration<RoleModules>
    {
        public void Configure(EntityTypeBuilder<RoleModules> builder)
        {
            builder.HasKey(mr => mr.ModuleRoleId);
        }
    }
}
