# ⚙️ SETUP – Guía de Instalación y Configuración
Este documento explica cómo instalar, configurar y ejecutar el proyecto Control de Tareas en un entorno local.

# 1. Requisitos previos
- Windows 10/11
- Visual Studio 2022 (v17.8 o superior)
- .NET SDK 8.0
- SQL Server 2019 o 2022
- SSMS (SQL Server Management Studio)
- Git

# 2. Clonar el repositorio
git clone https://github.com/Maynor2021/Control-De-Tareas.git
Abrir el archivo: Control De Tareas.sln

# 3. Configuración de la Base de Datos
Editar el archivo appsettings.json y agregar tu cadena local:
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR\\INSTANCIA;Database=ControlDeTareasDB;User Id=USUARIO;Password=PASSWORD;TrustServerCertificate=true"
}
Este archivo es local y NO debe subirse al repositorio.

# 4. Ejecutar Migraciones
Abrir Package Manager Console y ejecutar:
Update-Database

Esto crea las tablas:
Users, Roles, UserRoles, Courses, Tareas, Submissions, CourseOfferings, Enrollments, Periods, Grades, Announcements, AuditLogs, SubmissionFiles.

# 5. Poblar Base de Datos con Datos de Prueba
Ejecutar SeedData.sql en SSMS.
Ruta del archivo: Control De Tareas/Database/SeedData.sql

Este script incluye:
- 3 Roles (Administrador, Profesor, Estudiante)
- 13 Usuarios (1 admin, 4 profesores, 8 estudiantes)
- 6 Cursos con ofertas académicas
- 18 Tareas distribuidas en los cursos
- Inscripciones completas para todos los estudiantes
- Entregas y calificaciones realistas

Usuarios de prueba:
Admin: admin / TempPass123
Profesor: mgonzalez / TempPass123
Estudiante: est1 / TempPass123

# 6. Estructura del Proyecto
Controllers/
    HomeController.cs
    DashboardController.cs
    AccountController.cs
    CursosController.cs
    TareasController.cs

Data/Entitys/
    Users.cs
    Roles.cs
    UserRoles.cs
    Courses.cs
    Tareas.cs
    Submissions.cs
    CourseOfferings.cs
    Enrollments.cs
    Periods.cs
    Grades.cs
    Announcements.cs
    AuditLogs.cs
    SubmissionFiles.cs

Data/Configurations/
    UsersConfig.cs
    RolesConfig.cs
    CoursesConfig.cs
    TareasConfig.cs
    UserRolesConfig.cs
    SubmissionsConfig.cs
    CourseOfferingsConfig.cs
    EnrollmentsConfig.cs
    PeriodsConfig.cs
    GradesConfig.cs
    AnnouncementsConfig.cs
    AuditLogsConfig.cs
    SubmissionFilesConfig.cs

# 7. Variables de entorno (opcional)
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=<cadena segura>

# 8. Troubleshooting
❌ Login failed for user → Verifica usuario y contraseña SQL.
❌ Cannot open database → Ejecuta Update-Database.
❌ Error sp_getapplock en migraciones → Asegúrate de que SQL Server esté iniciado.
❌ Error por appsettings.json del compañero → Cada dev debe tener su cadena de conexión local.
❌ No hay datos después de migraciones → Ejecuta SeedData.sql completo.
❌ Las tablas no coinciden con las entidades → Revisa que las configuraciones EF estén correctas.

# 9. Comentarios en Código
Se documentaron:
- Controladores
- Entidades
- ViewModels
- Servicios
- Configuraciones EF Core
- DbContext

Documentación agregada con XML Comments.

# 10. Onboarding
1. Clonar el repositorio
2. Configurar la cadena local en appsettings.json
3. Ejecutar Update-Database
4. Ejecutar SeedData.sql
5. Ejecutar el proyecto
6. Iniciar sesión con los usuarios de prueba

# ✔ Proyecto listo para desarrollo colaborativo
