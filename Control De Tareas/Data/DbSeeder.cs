using Control_De_Tareas.Data.Entitys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Control_De_Tareas.Data
{
    public class DbSeeder
    {
        private readonly Context _context;
        private readonly ILogger<DbSeeder> _logger;

        public DbSeeder(Context context, ILogger<DbSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando proceso de seeding...");

                // Verificar si ya existen datos para evitar duplicados
                if (await _context.Roles.AnyAsync())
                {
                    _logger.LogInformation("La base de datos ya contiene datos. Saltando seeding.");
                    return;
                }

                await SeedRoles();
                await SeedUsers();
                await SeedCourses();
                await SeedPeriods();
                await SeedCourseOfferings();
                await SeedEnrollments();
                await SeedTareas();

                _logger.LogInformation("Seeding completado exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el proceso de seeding.");
                throw;
            }
        }

        private async Task SeedRoles()
        {
            var roles = new List<Roles>
            {
                new Roles { Name = "Administrador", Description = "Administrador del sistema", CreatedAt = DateTime.Now },
                new Roles { Name = "Profesor", Description = "Profesor que imparte cursos", CreatedAt = DateTime.Now },
                new Roles { Name = "Estudiante", Description = "Estudiante que toma cursos", CreatedAt = DateTime.Now }
            };

            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Roles creados: Administrador, Profesor, Estudiante");
        }

        private async Task SeedUsers()
        {
            // Obtener roles
            var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Administrador");
            var profesorRole = await _context.Roles.FirstAsync(r => r.Name == "Profesor");
            var estudianteRole = await _context.Roles.FirstAsync(r => r.Name == "Estudiante");

            // Crear usuarios
            var users = new List<Users>();

            // Función para hashear passwords usando .NET integrado
            string HashPassword(string password)
            {
                // Generar salt
                byte[] salt = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                // Hashear la contraseña
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                return $"{Convert.ToBase64String(salt)}.{hashed}";
            }

            // 1. Admin
            var admin = new Users
            {
                UserName = "admin",
                FullName = "Administrador Principal",
                Email = "admin@test.com",
                PasswordHash = HashPassword("Admin123!"),
                CreatedAt = DateTime.Now,
                IsEnabled = true
            };
            users.Add(admin);

            // 2. Profesores
            var profesor1 = new Users
            {
                UserName = "profesor1",
                FullName = "María González",
                Email = "maria.gonzalez@test.com",
                PasswordHash = HashPassword("Profesor123!"),
                CreatedAt = DateTime.Now,
                IsEnabled = true
            };
            users.Add(profesor1);

            var profesor2 = new Users
            {
                UserName = "profesor2",
                FullName = "Carlos Rodríguez",
                Email = "carlos.rodriguez@test.com",
                PasswordHash = HashPassword("Profesor123!"),
                CreatedAt = DateTime.Now,
                IsEnabled = true
            };
            users.Add(profesor2);

            // 3. Estudiantes
            var estudiante1 = new Users
            {
                UserName = "estudiante1",
                FullName = "Ana Martínez",
                Email = "ana.martinez@test.com",
                PasswordHash = HashPassword("Estudiante123!"),
                CreatedAt = DateTime.Now,
                IsEnabled = true
            };
            users.Add(estudiante1);

            var estudiante2 = new Users
            {
                UserName = "estudiante2",
                FullName = "Luis Hernández",
                Email = "luis.hernandez@test.com",
                PasswordHash = HashPassword("Estudiante123!"),
                CreatedAt = DateTime.Now,
                IsEnabled = true
            };
            users.Add(estudiante2);

            var estudiante3 = new Users
            {
                UserName = "estudiante3",
                FullName = "Sofia Ramírez",
                Email = "sofia.ramirez@test.com",
                PasswordHash = HashPassword("Estudiante123!"),
                CreatedAt = DateTime.Now,
                IsEnabled = true
            };
            users.Add(estudiante3);

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Asignar roles
            var userRoles = new List<UserRoles>
            {
                new UserRoles { UserId = admin.Id, RoleId = adminRole.Id, AssignedAt = DateTime.Now },
                new UserRoles { UserId = profesor1.Id, RoleId = profesorRole.Id, AssignedAt = DateTime.Now },
                new UserRoles { UserId = profesor2.Id, RoleId = profesorRole.Id, AssignedAt = DateTime.Now },
                new UserRoles { UserId = estudiante1.Id, RoleId = estudianteRole.Id, AssignedAt = DateTime.Now },
                new UserRoles { UserId = estudiante2.Id, RoleId = estudianteRole.Id, AssignedAt = DateTime.Now },
                new UserRoles { UserId = estudiante3.Id, RoleId = estudianteRole.Id, AssignedAt = DateTime.Now }
            };

            await _context.UserRoles.AddRangeAsync(userRoles);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuarios creados: 1 Admin, 2 Profesores, 3 Estudiantes");
        }

        private async Task SeedCourses()
        {
            var courses = new List<Courses>
            {
                new Courses
                {
                    Code = "MAT101",
                    Title = "Matemáticas Básicas",
                    Description = "Curso introductorio de matemáticas para estudiantes de primer año",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new Courses
                {
                    Code = "LEN102",
                    Title = "Lenguaje y Literatura",
                    Description = "Curso de lenguaje, gramática y análisis literario",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                }
            };

            await _context.Courses.AddRangeAsync(courses);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cursos creados: MAT101, LEN102");
        }

        private async Task SeedPeriods()
        {
            var period = new Periods
            {
                Name = "Primer Semestre 2025",
                StartDate = new DateTime(2025, 1, 15),
                EndDate = new DateTime(2025, 6, 15),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _context.Periods.AddAsync(period);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Periodo académico creado");
        }

        private async Task SeedCourseOfferings()
        {
            var profesor1 = await _context.Users.FirstAsync(u => u.UserName == "profesor1");
            var profesor2 = await _context.Users.FirstAsync(u => u.UserName == "profesor2");
            var matCourse = await _context.Courses.FirstAsync(c => c.Code == "MAT101");
            var lenCourse = await _context.Courses.FirstAsync(c => c.Code == "LEN102");
            var period = await _context.Periods.FirstAsync();

            var offerings = new List<CourseOfferings>
            {
                new CourseOfferings
                {
                    CourseId = matCourse.Id,
                    ProfessorId = profesor1.Id,
                    PeriodId = period.Id,
                    Section = "Sección 01",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new CourseOfferings
                {
                    CourseId = lenCourse.Id,
                    ProfessorId = profesor2.Id,
                    PeriodId = period.Id,
                    Section = "Sección 01",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                }
            };

            await _context.CourseOfferings.AddRangeAsync(offerings);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Ofertas de cursos creadas y asignadas a profesores");
        }

        private async Task SeedEnrollments()
        {
            var estudiantes = await _context.Users
                .Where(u => u.UserName.StartsWith("estudiante"))
                .ToListAsync();

            var courseOfferings = await _context.CourseOfferings.ToListAsync();

            var enrollments = new List<Enrollments>();

            // Inscribir estudiantes en cursos
            foreach (var estudiante in estudiantes)
            {
                // Cada estudiante se inscribe en Matemáticas
                enrollments.Add(new Enrollments
                {
                    CourseOfferingId = courseOfferings[0].Id, // MAT101
                    StudentId = estudiante.Id,
                    EnrolledAt = DateTime.Now,
                    Status = "Active"
                });

                // Estudiantes 1 y 2 también se inscriben en Lenguaje
                if (estudiante.UserName == "estudiante1" || estudiante.UserName == "estudiante2")
                {
                    enrollments.Add(new Enrollments
                    {
                        CourseOfferingId = courseOfferings[1].Id, // LEN102
                        StudentId = estudiante.Id,
                        EnrolledAt = DateTime.Now,
                        Status = "Active"
                    });
                }
            }

            await _context.Enrollments.AddRangeAsync(enrollments);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Inscripciones de estudiantes creadas");
        }

        private async Task SeedTareas()
        {
            var courseOfferings = await _context.CourseOfferings.ToListAsync();
            var profesor1 = await _context.Users.FirstAsync(u => u.UserName == "profesor1");
            var profesor2 = await _context.Users.FirstAsync(u => u.UserName == "profesor2");

            var tareas = new List<Tareas>
            {
                // Tareas para Matemáticas
                new Tareas
                {
                    CourseOfferingId = courseOfferings[0].Id,
                    Title = "Tarea 1: Álgebra Básica",
                    Description = "Resolver los ejercicios de álgebra de la página 50 del libro de texto",
                    DueDate = DateTime.Now.AddDays(7),
                    CreatedBy = profesor1.Id,
                    MaxScore = 100
                },
                new Tareas
                {
                    CourseOfferingId = courseOfferings[0].Id,
                    Title = "Tarea 2: Geometría Planar",
                    Description = "Problemas de geometría plana - ejercicios 1 al 20",
                    DueDate = DateTime.Now.AddDays(14),
                    CreatedBy = profesor1.Id,
                    MaxScore = 100
                },
                // Tareas para Lenguaje
                new Tareas
                {
                    CourseOfferingId = courseOfferings[1].Id,
                    Title = "Análisis Literario: Don Quijote",
                    Description = "Leer el capítulo 1 y realizar un análisis de los personajes principales",
                    DueDate = DateTime.Now.AddDays(10),
                    CreatedBy = profesor2.Id,
                    MaxScore = 100
                }
            };

            await _context.Tareas.AddRangeAsync(tareas);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Tareas de ejemplo creadas");
        }
    }
}