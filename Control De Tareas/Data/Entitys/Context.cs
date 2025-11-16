using Microsoft.EntityFrameworkCore;
using Control_De_Tareas.Data.Entitys;

namespace Control_De_Tareas.Data.Entitys
{
    /// <summary>
    /// Contexto principal de Entity Framework Core para la aplicación Control de Tareas.
    /// Gestiona las entidades del sistema y aplica las configuraciones definidas.
    /// </summary>
    public class Context : DbContext
    {
        /// <summary>
        /// Constructor del contexto que recibe las opciones de configuración.
        /// </summary>
        /// <param name="options">Opciones del contexto proporcionadas por DI.</param>
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        /// <summary>
        /// Tabla de usuarios del sistema.
        /// </summary>
        public DbSet<Users> Users { get; set; }

        /// <summary>
        /// Tabla de roles existentes (admin, profesor, estudiante).
        /// </summary>
        public DbSet<Roles> Roles { get; set; }

        /// <summary>
        /// Tabla que representa la relación muchos-a-muchos entre usuarios y roles.
        /// </summary>
        public DbSet<UserRoles> UserRoles { get; set; }

        /// <summary>
        /// Tabla de tareas asignadas dentro de los cursos.
        /// </summary>
        public DbSet<Tareas> Tareas { get; set; }

        /// <summary>
        /// Tabla de entregas realizadas por los estudiantes.
        /// </summary>
        public DbSet<Submissions> Submissions { get; set; }

        /// <summary>
        /// Tabla de cursos administrados en la plataforma.
        /// </summary>
        public DbSet<Courses> Courses { get; set; }

        /// <summary>
        /// Método encargado de aplicar las configuraciones detalladas de las entidades,
        /// tales como restricciones, relaciones, claves y reglas de validación.
        /// </summary>
        /// <param name="modelBuilder">Builder para configurar el modelo de EF Core.</param>
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
