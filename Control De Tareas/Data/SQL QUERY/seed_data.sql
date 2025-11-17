-- =====================================================
-- SCRIPT COMPLETO: LIMPIEZA Y INSERCIÓN DE DATOS
-- =====================================================
USE TaskManagerDb;
/*
INSTRUCCIONES:
1. Este script ELIMINARÁ todos los datos existentes
2. Insertará nuevos datos estáticos y congruentes
3. Asegura que todos los estudiantes tengan inscripciones
*/

PRINT '=== INICIANDO LIMPIEZA Y INSERCIÓN COMPLETA ===';

-- =====================================================
-- LIMPIEZA COMPLETA DE DATOS (EN ORDEN CORRECTO POR FK)
-- =====================================================

PRINT '1. LIMPIANDO DATOS EXISTENTES...';

-- Deshabilitar constraints temporalmente para limpieza
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- Eliminar datos en orden correcto (evitar violaciones FK)
DELETE FROM AuditLogs;
DELETE FROM Grades;
DELETE FROM Submissions;
DELETE FROM Tasks;
DELETE FROM Announcements;
DELETE FROM Enrollments;
DELETE FROM CourseOfferings;
DELETE FROM UserRoles;
DELETE FROM Users;
DELETE FROM Periods;
DELETE FROM Courses;
DELETE FROM Roles;

-- Reiniciar identidades
DBCC CHECKIDENT ('AuditLogs', RESEED, 0);
DBCC CHECKIDENT ('Grades', RESEED, 0);
DBCC CHECKIDENT ('Submissions', RESEED, 0);
DBCC CHECKIDENT ('Tasks', RESEED, 0);
DBCC CHECKIDENT ('Announcements', RESEED, 0);
DBCC CHECKIDENT ('Enrollments', RESEED, 0);
DBCC CHECKIDENT ('CourseOfferings', RESEED, 0);
DBCC CHECKIDENT ('UserRoles', RESEED, 0);
DBCC CHECKIDENT ('Users', RESEED, 0);
DBCC CHECKIDENT ('Periods', RESEED, 0);
DBCC CHECKIDENT ('Courses', RESEED, 0);
DBCC CHECKIDENT ('Roles', RESEED, 0);

-- Habilitar constraints nuevamente
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';

PRINT 'Limpieza completada. Todas las tablas vacías.';

-- =====================================================
-- INSERCIÓN COMPLETA DE DATOS ESTÁTICOS
-- =====================================================

PRINT '2. INSERTANDO NUEVOS DATOS...';

-- 1. Insertar Roles
INSERT INTO Roles (Name, Description, CreatedAt) VALUES
('Administrador', 'Administrador del sistema', GETDATE()),
('Profesor', 'Profesor que imparte cursos', GETDATE()),
('Estudiante', 'Estudiante que toma cursos', GETDATE());

PRINT '- Roles insertados: 3';

-- 2. Insertar Usuarios
INSERT INTO Users (UserName, FullName, Email, PasswordHash, CreatedAt, IsEnabled) VALUES
-- Administrador
('admin', 'Administrador Principal', 'admin@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1),

-- Profesores
('mgonzalez', 'María González', 'maria.gonzalez@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1),
('crodriguez', 'Carlos Rodríguez', 'carlos.rodriguez@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1),
('alopez', 'Ana López', 'ana.lopez@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1),
('jmartinez', 'José Martínez', 'jose.martinez@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1),

-- Estudiantes (8 estudiantes)
('est1', 'Ana Martínez', 'ana.martinez@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1),
('est2', 'Luis Hernández', 'luis.hernandez@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1),
('est3', 'Sofia Ramírez', 'sofia.ramirez@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1),
('est4', 'Carlos García', 'carlos.garcia@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1),
('est5', 'Marta López', 'marta.lopez@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1),
('est6', 'Pedro Sánchez', 'pedro.sanchez@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1),
('est7', 'Laura Díaz', 'laura.diaz@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1),
('est8', 'David Torres', 'david.torres@sistema.com', 'VGVtcFBhc3Mv.Q5hH6zsqGwJgNSzJ2zvrKjWZUwR9k6hMzR9k6hMzR9k=', GETDATE(), 1);

PRINT '- Usuarios insertados: 13 (1 admin, 4 profesores, 8 estudiantes)';

-- 3. Asignar Roles a Usuarios
INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
SELECT u.Id, r.Id, GETDATE()
FROM Users u
CROSS JOIN Roles r
WHERE 
    (u.UserName = 'admin' AND r.Name = 'Administrador') OR
    (u.UserName IN ('mgonzalez', 'crodriguez', 'alopez', 'jmartinez') AND r.Name = 'Profesor') OR
    (u.UserName LIKE 'est%' AND r.Name = 'Estudiante');

PRINT '- Roles de usuarios asignados: 13';

-- 4. Insertar Cursos
INSERT INTO Courses (Code, Title, Description, CreatedAt, IsActive) VALUES
('MAT101', 'Matemáticas Básicas', 'Álgebra, geometría y cálculo básico', GETDATE(), 1),
('LEN102', 'Lenguaje y Literatura', 'Gramática, redacción y análisis literario', GETDATE(), 1),
('FIS201', 'Física General', 'Mecánica, termodinámica y electromagnetismo', GETDATE(), 1),
('QUI202', 'Química Orgánica', 'Compuestos orgánicos y reacciones químicas', GETDATE(), 1),
('HIS301', 'Historia Universal', 'Historia mundial desde la antigüedad', GETDATE(), 1),
('BIO302', 'Biología Celular', 'Estructura y función celular', GETDATE(), 1);

PRINT '- Cursos insertados: 6';

-- 5. Insertar Periodos Académicos
INSERT INTO Periods (Name, StartDate, EndDate, IsActive, CreatedAt) VALUES
('Primer Semestre 2025', '2025-01-15', '2025-06-15', 1, GETDATE()),
('Segundo Semestre 2025', '2025-07-01', '2025-12-15', 0, GETDATE());

PRINT '- Periodos insertados: 2';

-- 6. Insertar Ofertas de Cursos (CourseOfferings)
INSERT INTO CourseOfferings (CourseId, ProfessorId, PeriodId, Section, CreatedAt, IsActive)
SELECT 
    c.Id,
    p.Id,
    per.Id,
    CASE 
        WHEN c.Code = 'MAT101' THEN 'MAT101-01'
        WHEN c.Code = 'LEN102' THEN 'LEN102-01'
        WHEN c.Code = 'FIS201' THEN 'FIS201-01'
        WHEN c.Code = 'QUI202' THEN 'QUI202-01'
        WHEN c.Code = 'HIS301' THEN 'HIS301-01'
        WHEN c.Code = 'BIO302' THEN 'BIO302-01'
    END,
    GETDATE(),
    1
FROM Courses c
CROSS JOIN (SELECT Id FROM Periods WHERE IsActive = 1) per
CROSS JOIN (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) as rn 
    FROM Users 
    WHERE UserName IN ('mgonzalez', 'crodriguez', 'alopez', 'jmartinez')
) p
WHERE 
    (c.Code = 'MAT101' AND p.rn = 1) OR
    (c.Code = 'LEN102' AND p.rn = 2) OR
    (c.Code = 'FIS201' AND p.rn = 3) OR
    (c.Code = 'QUI202' AND p.rn = 4) OR
    (c.Code = 'HIS301' AND p.rn = 2) OR
    (c.Code = 'BIO302' AND p.rn = 3);

PRINT '- Ofertas de cursos insertadas: 6';

-- 7. Insertar Inscripciones (Enrollments) - TODOS LOS ESTUDIANTES EN TODOS LOS CURSOS
INSERT INTO Enrollments (CourseOfferingId, StudentId, EnrolledAt, Status)
SELECT 
    co.Id, 
    u.Id, 
    DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 30, GETDATE()), -- Fecha aleatoria en los últimos 30 días
    'Active'
FROM CourseOfferings co
CROSS JOIN Users u
WHERE u.UserName LIKE 'est%'
ORDER BY u.Id, co.Id;

PRINT '- Inscripciones insertadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- 8. Insertar Tareas (Tasks) - 3 tareas por curso
INSERT INTO Tasks (CourseOfferingId, Title, Description, DueDate, CreatedBy, MaxScore)
-- Tareas para MAT101-01
SELECT co.Id, 
    'Tarea 1: Álgebra Lineal', 
    'Resolver ejercicios de sistemas de ecuaciones lineales', 
    '2025-02-15', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'MAT101-01'

UNION ALL

SELECT co.Id, 
    'Tarea 2: Cálculo Diferencial', 
    'Problemas de límites y derivadas', 
    '2025-03-01', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'MAT101-01'

UNION ALL

SELECT co.Id, 
    'Tarea 3: Geometría Analítica', 
    'Problemas de rectas y planos en el espacio', 
    '2025-03-20', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'MAT101-01'

UNION ALL

-- Tareas para LEN102-01
SELECT co.Id, 
    'Tarea 1: Análisis Literario', 
    'Analizar obra "Cien años de soledad"', 
    '2025-02-10', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'LEN102-01'

UNION ALL

SELECT co.Id, 
    'Tarea 2: Redacción Académica', 
    'Escribir ensayo sobre cambio climático', 
    '2025-03-05', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'LEN102-01'

UNION ALL

SELECT co.Id, 
    'Tarea 3: Gramática Avanzada', 
    'Ejercicios de sintaxis y morfología', 
    '2025-03-25', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'LEN102-01'

UNION ALL

-- Tareas para FIS201-01
SELECT co.Id, 
    'Tarea 1: Leyes de Newton', 
    'Problemas de aplicación de las leyes de Newton', 
    '2025-02-20', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'FIS201-01'

UNION ALL

SELECT co.Id, 
    'Tarea 2: Energía y Trabajo', 
    'Ejercicios de conservación de energía', 
    '2025-03-10', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'FIS201-01'

UNION ALL

SELECT co.Id, 
    'Tarea 3: Electromagnetismo', 
    'Problemas de campos eléctricos y magnéticos', 
    '2025-03-30', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'FIS201-01'

UNION ALL

-- Tareas para QUI202-01
SELECT co.Id, 
    'Tarea 1: Enlaces Químicos', 
    'Identificar tipos de enlaces en compuestos', 
    '2025-02-18', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'QUI202-01'

UNION ALL

SELECT co.Id, 
    'Tarea 2: Reacciones Orgánicas', 
    'Balancear ecuaciones de reacciones orgánicas', 
    '2025-03-08', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'QUI202-01'

UNION ALL

SELECT co.Id, 
    'Tarea 3: Laboratorio Virtual', 
    'Simulación de experimentos químicos', 
    '2025-03-28', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'QUI202-01'

UNION ALL

-- Tareas para HIS301-01
SELECT co.Id, 
    'Tarea 1: Revolución Industrial', 
    'Ensayo sobre impactos de la Revolución Industrial', 
    '2025-02-25', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'HIS301-01'

UNION ALL

SELECT co.Id, 
    'Tarea 2: Guerras Mundiales', 
    'Análisis comparativo de las guerras mundiales', 
    '2025-03-15', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'HIS301-01'

UNION ALL

SELECT co.Id, 
    'Tarea 3: Civilizaciones Antiguas', 
    'Estudio de civilizaciones mesoamericanas', 
    '2025-04-05', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'HIS301-01'

UNION ALL

-- Tareas para BIO302-01
SELECT co.Id, 
    'Tarea 1: Estructura Celular', 
    'Diagramar y describir organelos celulares', 
    '2025-02-22', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'BIO302-01'

UNION ALL

SELECT co.Id, 
    'Tarea 2: ADN y Genética', 
    'Problemas de herencia genética', 
    '2025-03-12', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'BIO302-01'

UNION ALL

SELECT co.Id, 
    'Tarea 3: Ecosistemas', 
    'Análisis de cadenas alimenticias', 
    '2025-04-02', 
    co.ProfessorId, 
    100
FROM CourseOfferings co WHERE co.Section = 'BIO302-01';

PRINT '- Tareas insertadas: 18 (3 por curso)';

-- 9. Insertar Entregas (Submissions) - TODOS los estudiantes entregan TODAS las tareas
INSERT INTO Submissions (TaskId, StudentId, SubmittedAt, Comments, Status, CurrentGrade)
SELECT 
    t.Id,
    e.StudentId,
    DATEADD(DAY, -2, t.DueDate), -- Todos entregan 2 días antes del vencimiento
    CASE 
        WHEN t.Title LIKE '%1:%' THEN 'Completé todos los ejercicios solicitados'
        WHEN t.Title LIKE '%2:%' THEN 'Adjunto mi trabajo, espero cumpla con los requisitos'
        ELSE 'Entregando la tarea dentro del plazo establecido'
    END,
    'Submitted',
    NULL
FROM Tasks t
INNER JOIN Enrollments e ON t.CourseOfferingId = e.CourseOfferingId
ORDER BY t.Id, e.StudentId;

PRINT '- Entregas insertadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- 10. Insertar Calificaciones (Grades) - TODAS las entregas calificadas
INSERT INTO Grades (SubmissionId, GraderId, Score, Feedback, GradedAt)
SELECT 
    s.Id,
    co.ProfessorId,
    -- Calificaciones realistas basadas en el estudiante y curso
    CASE 
        WHEN u.UserName = 'est1' THEN 95.0  -- Estudiante destacado
        WHEN u.UserName = 'est2' THEN 88.0
        WHEN u.UserName = 'est3' THEN 92.0
        WHEN u.UserName = 'est4' THEN 85.0
        WHEN u.UserName = 'est5' THEN 78.0
        WHEN u.UserName = 'est6' THEN 82.0
        WHEN u.UserName = 'est7' THEN 90.0
        WHEN u.UserName = 'est8' THEN 87.0
        ELSE 80.0
    END,
    CASE 
        WHEN u.UserName = 'est1' THEN 'Excelente trabajo, muy detallado y completo'
        WHEN u.UserName = 'est2' THEN 'Buen trabajo, algunos errores menores por corregir'
        WHEN u.UserName = 'est3' THEN 'Muy bien organizado y presentado'
        WHEN u.UserName = 'est4' THEN 'Aceptable, necesita mejorar la presentación'
        WHEN u.UserName = 'est5' THEN 'Requiere más atención a los detalles'
        WHEN u.UserName = 'est6' THEN 'Buen contenido, mejorar organización'
        WHEN u.UserName = 'est7' THEN 'Excelente desarrollo de conceptos'
        WHEN u.UserName = 'est8' THEN 'Sólido trabajo, continuar así'
        ELSE 'Feedback general positivo'
    END,
    DATEADD(DAY, 1, s.SubmittedAt) -- Calificado 1 día después de entrega
FROM Submissions s
INNER JOIN Tasks t ON s.TaskId = t.Id
INNER JOIN CourseOfferings co ON t.CourseOfferingId = co.Id
INNER JOIN Users u ON s.StudentId = u.Id
ORDER BY s.Id;

PRINT '- Calificaciones insertadas: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- 11. Actualizar las calificaciones actuales en Submissions
UPDATE Submissions 
SET CurrentGrade = g.Score
FROM Submissions s
INNER JOIN Grades g ON s.Id = g.SubmissionId;

PRINT '- Calificaciones actualizadas en entregas';

-- 12. Insertar Anuncios (Announcements)
INSERT INTO Announcements (CourseOfferingId, Title, Body, PostedAt, PostedBy)
-- Anuncios para MAT101-01
SELECT co.Id, 
    'Bienvenida al Curso de Matemáticas', 
    'Bienvenidos al curso de Matemáticas Básicas. Revisen el sílabo en la plataforma.', 
    '2025-01-16 08:00:00', 
    co.ProfessorId
FROM CourseOfferings co WHERE co.Section = 'MAT101-01'

UNION ALL

SELECT co.Id, 
    'Recordatorio: Tarea 1', 
    'Recuerden que la Tarea 1 vence el 15 de febrero. No olviden entregarla.', 
    '2025-02-10 10:00:00', 
    co.ProfessorId
FROM CourseOfferings co WHERE co.Section = 'MAT101-01'

UNION ALL

-- Anuncios para LEN102-01
SELECT co.Id, 
    'Inicio de Clases de Lenguaje', 
    'Estimados estudiantes, las clases inician el lunes 20 de enero. Favor revisar material.', 
    '2025-01-17 09:00:00', 
    co.ProfessorId
FROM CourseOfferings co WHERE co.Section = 'LEN102-01'

UNION ALL

-- Anuncios para FIS201-01
SELECT co.Id, 
    'Laboratorio de Física', 
    'El primer laboratorio será el 25 de enero. Traer calculadora y cuaderno de apuntes.', 
    '2025-01-18 11:00:00', 
    co.ProfessorId
FROM CourseOfferings co WHERE co.Section = 'FIS201-01'

UNION ALL

-- Anuncios para QUI202-01
SELECT co.Id, 
    'Material de Química', 
    'El libro de texto ya está disponible en la biblioteca.', 
    '2025-01-19 14:00:00', 
    co.ProfessorId
FROM CourseOfferings co WHERE co.Section = 'QUI202-01'

UNION ALL

-- Anuncios para HIS301-01
SELECT co.Id, 
    'Documentales Recomendados', 
    'Lista de documentales históricos disponibles en la plataforma.', 
    '2025-01-20 16:00:00', 
    co.ProfessorId
FROM CourseOfferings co WHERE co.Section = 'HIS301-01'

UNION ALL

-- Anuncios para BIO302-01
SELECT co.Id, 
    'Salida de Campo', 
    'Programación de salida de campo para observación de ecosistemas.', 
    '2025-01-21 13:00:00', 
    co.ProfessorId
FROM CourseOfferings co WHERE co.Section = 'BIO302-01';

PRINT '- Anuncios insertados: 7';

-- =====================================================
-- VERIFICACIÓN FINAL DE DATOS INSERTADOS
-- =====================================================

PRINT '3. VERIFICANDO DATOS INSERTADOS...';

SELECT 
    'Roles' as Tabla, COUNT(*) as Registros FROM Roles
UNION ALL SELECT 'Users', COUNT(*) FROM Users
UNION ALL SELECT 'UserRoles', COUNT(*) FROM UserRoles
UNION ALL SELECT 'Courses', COUNT(*) FROM Courses
UNION ALL SELECT 'Periods', COUNT(*) FROM Periods
UNION ALL SELECT 'CourseOfferings', COUNT(*) FROM CourseOfferings
UNION ALL SELECT 'Enrollments', COUNT(*) FROM Enrollments
UNION ALL SELECT 'Tasks', COUNT(*) FROM Tasks
UNION ALL SELECT 'Submissions', COUNT(*) FROM Submissions
UNION ALL SELECT 'Grades', COUNT(*) FROM Grades
UNION ALL SELECT 'Announcements', COUNT(*) FROM Announcements
ORDER BY Tabla;

-- Verificación específica de inscripciones
PRINT '=== VERIFICACIÓN DE INSCRIPCIONES ===';

SELECT 
    c.Code as Curso,
    c.Title as NombreCurso,
    COUNT(DISTINCT e.StudentId) as EstudiantesInscritos,
    COUNT(DISTINCT t.Id) as TareasCreadas,
    COUNT(DISTINCT s.Id) as EntregasTotales,
    COUNT(DISTINCT g.Id) as Calificaciones
FROM Courses c
JOIN CourseOfferings co ON c.Id = co.CourseId
LEFT JOIN Enrollments e ON co.Id = e.CourseOfferingId
LEFT JOIN Tasks t ON co.Id = t.CourseOfferingId
LEFT JOIN Submissions s ON t.Id = s.TaskId
LEFT JOIN Grades g ON s.Id = g.SubmissionId
GROUP BY c.Code, c.Title
ORDER BY c.Code;

PRINT '=== SCRIPT COMPLETADO EXITOSAMENTE ===';
PRINT 'RESUMEN:';
PRINT '- Todos los datos anteriores fueron ELIMINADOS';
PRINT '- Nuevos datos estáticos insertados';
PRINT '- TODOS los estudiantes están inscritos en TODOS los cursos';
PRINT '- TODAS las tareas fueron entregadas por TODOS los estudiantes';
PRINT '- TODAS las entregas fueron calificadas';
PRINT '- Integridad referencial 100% garantizada';