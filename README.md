Plataforma de Control de Tareas y Calificaciones
Objetivo
Desarrollar una plataforma web mínima (MVP) que permita gestionar cursos, tareas, entregas y calificaciones,
facilitando el flujo docente-estudiante para la creación de actividades, envío de entregas y evaluación.
El objetivo es construir una solución funcional y escalable para presentar en la clase de Desarrollo Web,
empleando C# (MVC), Entity Framework Core y SQL Server.
Descripción del sistema (Alcance)
La Plataforma de Control de Tareas y Calificaciones es un sistema web orientado a la gestión académica básica.
Alcance para la entrega (MVP):
- Actores: Administrador, Profesor, Estudiante.
- Funcionalidades principales: Registro/Login, gestión básica de cursos, creación de tareas, entrega de tareas por parte
de estudiantes (upload de archivo), calificación por parte de profesores (nota y feedback), y consulta de historial de
calificaciones por estudiante.
- Restricciones: Interfaz basada en Razor Views (MVC), sin notificaciones en tiempo real ni integraciones externas para
mantener el alcance reducido.
- Entregables: Código fuente, migraciones/DDL y documentación breve.
Casos de uso (Mínimos para el MVP)
CU-01: Registro / Login: Usuario se registra o es creado por Admin y puede iniciar sesión.
CU-02: Crear Curso: Profesor crea y administra cursos; el curso queda disponible para inscripciones.
CU-03: Inscribirse a Curso: Estudiante se inscribe en un curso o es inscrito por Admin.
CU-04: Crear Tarea: Profesor crea tarea con título, descripción y fecha límite.
CU-05: Entregar Tarea: Estudiante envía archivos y comentarios como entrega de la tarea.
CU-06: Calificar Tarea: Profesor asigna nota y feedback a una entrega; puede registrarse historial.
CU-07: Ver Calificaciones: Estudiante consulta sus calificaciones y feedback por tarea.
CU-08: Gestión de Usuarios: Admin crea/edita roles y usuarios; asigna permisos básicos.
