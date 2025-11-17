using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Control_De_Tareas.Data.Entitys;

namespace Control_De_Tareas.Data.Configurations
{
    public class TareasConfig : IEntityTypeConfiguration<Tareas>
    {
        public void Configure(EntityTypeBuilder<Tareas> builder)
        {
            builder.ToTable("Tasks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(t => t.Description)
                   .HasMaxLength(500);

            builder.Property(t => t.DueDate)
                   .IsRequired();

            builder.Property(t => t.MaxScore)
                   .HasColumnType("decimal(5,2)")
                   .HasDefaultValue(100);

            builder.Property(t => t.IsSoftDeleted)
                   .HasDefaultValue(false);

            builder.HasOne(t => t.CourseOffering)
                   .WithMany(co => co.Tareas)
                   .HasForeignKey(t => t.CourseOfferingId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.CreatedByUser)
                   .WithMany(u => u.CreatedTasks)
                   .HasForeignKey(t => t.CreatedBy)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class CoursesConfig : IEntityTypeConfiguration<Courses>
    {
        public void Configure(EntityTypeBuilder<Courses> builder)
        {
            builder.ToTable("Courses");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Code)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.Title)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(c => c.Description)
                   .HasMaxLength(500);

            builder.Property(c => c.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.IsActive)
                   .HasDefaultValue(true);

            builder.Property(c => c.IsSoftDeleted)
                   .HasDefaultValue(false);
        }
    }

    public class RolesConfig : IEntityTypeConfiguration<Roles>
    {
        public void Configure(EntityTypeBuilder<Roles> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(r => r.Description)
                   .HasMaxLength(255);

            builder.Property(r => r.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(r => r.IsSoftDeleted)
                   .HasDefaultValue(false);

            builder.HasMany(r => r.UserRoles)
                   .WithOne(ur => ur.Role)
                   .HasForeignKey(ur => ur.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class UsersConfig : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.UserName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.FullName)
                   .HasMaxLength(150);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.HasIndex(u => u.Email)
                   .IsUnique();

            builder.Property(u => u.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(u => u.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(u => u.IsEnabled)
                   .HasDefaultValue(true);

            builder.Property(u => u.IsSoftDeleted)
                   .HasDefaultValue(false);

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
            builder.ToTable("UserRoles");

            builder.HasKey(ur => ur.Id);

            builder.Property(ur => ur.AssignedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(ur => ur.IsSoftDeleted)
                   .HasDefaultValue(false);

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
            builder.ToTable("Submissions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.SubmittedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(s => s.Comments)
                   .HasMaxLength(500);

            builder.Property(s => s.CurrentGrade)
                   .HasColumnType("decimal(5,2)");

            builder.Property(s => s.Status)
                   .HasMaxLength(50)
                   .HasDefaultValue("Submitted");

            builder.Property(s => s.IsSoftDeleted)
                   .HasDefaultValue(false);

            builder.HasOne(s => s.Task)
                   .WithMany(t => t.Submissions)
                   .HasForeignKey(s => s.TaskId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Student)
                   .WithMany(u => u.Submissions)
                   .HasForeignKey(s => s.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => s.TaskId);
            builder.HasIndex(s => s.StudentId);
            builder.HasIndex(s => s.SubmittedAt);
        }
    }

    public class PeriodsConfig : IEntityTypeConfiguration<Periods>
    {
        public void Configure(EntityTypeBuilder<Periods> builder)
        {
            builder.ToTable("Periods");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.StartDate)
                   .IsRequired();

            builder.Property(p => p.EndDate)
                   .IsRequired();

            builder.Property(p => p.IsActive)
                   .HasDefaultValue(true);

            builder.Property(p => p.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.IsSoftDeleted)
                   .HasDefaultValue(false);
        }
    }

    public class CourseOfferingsConfig : IEntityTypeConfiguration<CourseOfferings>
    {
        public void Configure(EntityTypeBuilder<CourseOfferings> builder)
        {
            builder.ToTable("CourseOfferings");

            builder.HasKey(co => co.Id);

            builder.Property(co => co.Section)
                   .HasMaxLength(10);

            builder.Property(co => co.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(co => co.IsActive)
                   .HasDefaultValue(true);

            builder.Property(co => co.IsSoftDeleted)
                   .HasDefaultValue(false);

            builder.HasOne(co => co.Course)
                   .WithMany()
                   .HasForeignKey(co => co.CourseId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(co => co.Professor)
                   .WithMany(u => u.CourseOfferings)
                   .HasForeignKey(co => co.ProfessorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(co => co.Period)
                   .WithMany(p => p.CourseOfferings)
                   .HasForeignKey(co => co.PeriodId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class EnrollmentsConfig : IEntityTypeConfiguration<Enrollments>
    {
        public void Configure(EntityTypeBuilder<Enrollments> builder)
        {
            builder.ToTable("Enrollments");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.EnrolledAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(e => e.Status)
                   .HasMaxLength(50)
                   .HasDefaultValue("Active");

            builder.Property(e => e.IsSoftDeleted)
                   .HasDefaultValue(false);

            builder.HasOne(e => e.CourseOffering)
                   .WithMany(co => co.Enrollments)
                   .HasForeignKey(e => e.CourseOfferingId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Student)
                   .WithMany(u => u.Enrollments)
                   .HasForeignKey(e => e.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class SubmissionFilesConfig : IEntityTypeConfiguration<SubmissionFiles>
    {
        public void Configure(EntityTypeBuilder<SubmissionFiles> builder)
        {
            builder.ToTable("SubmissionFiles");

            builder.HasKey(sf => sf.Id);

            builder.Property(sf => sf.FilePath)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(sf => sf.FileName)
                   .HasMaxLength(255);

            builder.Property(sf => sf.UploadedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(sf => sf.IsSoftDeleted)
                   .HasDefaultValue(false);

            builder.HasOne(sf => sf.Submission)
                   .WithMany(s => s.SubmissionFiles)
                   .HasForeignKey(sf => sf.SubmissionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class GradesConfig : IEntityTypeConfiguration<Grades>
    {
        public void Configure(EntityTypeBuilder<Grades> builder)
        {
            builder.ToTable("Grades");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Score)
                   .HasColumnType("decimal(5,2)")
                   .IsRequired();

            builder.Property(g => g.Feedback)
                   .HasMaxLength(500);

            builder.Property(g => g.GradedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(g => g.IsSoftDeleted)
                   .HasDefaultValue(false);

            builder.HasOne(g => g.Submission)
                   .WithMany(s => s.Grades)
                   .HasForeignKey(g => g.SubmissionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(g => g.Grader)
                   .WithMany(u => u.Grades)
                   .HasForeignKey(g => g.GraderId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class AnnouncementsConfig : IEntityTypeConfiguration<Announcements>
    {
        public void Configure(EntityTypeBuilder<Announcements> builder)
        {
            builder.ToTable("Announcements");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Title)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(a => a.Body);

            builder.Property(a => a.PostedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(a => a.IsSoftDeleted)
                   .HasDefaultValue(false);

            builder.HasOne(a => a.CourseOffering)
                   .WithMany(co => co.Announcements)
                   .HasForeignKey(a => a.CourseOfferingId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.PostedByUser)
                   .WithMany(u => u.Announcements)
                   .HasForeignKey(a => a.PostedBy)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class AuditLogsConfig : IEntityTypeConfiguration<AuditLogs>
    {
        public void Configure(EntityTypeBuilder<AuditLogs> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(al => al.Id);

            builder.Property(al => al.Action)
                   .HasMaxLength(100);

            builder.Property(al => al.Entity)
                   .HasMaxLength(100);

            builder.Property(al => al.Details);

            builder.Property(al => al.CreatedAt)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(al => al.IsSoftDeleted)
                   .HasDefaultValue(false);

            builder.HasOne(al => al.User)
                   .WithMany()
                   .HasForeignKey(al => al.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}