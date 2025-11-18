-- =====================================================
-- SUPER SCRIPT COMPLETO: VERIFICACIÓN Y ANÁLISIS AVANZADO
-- =====================================================
USE TaskManagerDb;

PRINT '===================================================================';
PRINT 'INICIANDO VERIFICACIÓN COMPLETA Y ANÁLISIS AVANZADO DE DATOS';
PRINT '===================================================================';
PRINT '';

-- =====================================================
-- 1. VERIFICACIÓN DE INTEGRIDAD REFERENCIAL CRÍTICA
-- =====================================================

PRINT '1. VERIFICACIÓN DE INTEGRIDAD REFERENCIAL';
PRINT '---------------------------------------------';

SELECT 
    CASE 
        WHEN Problema = 1 THEN 'ESTUDIANTES CON ENTREGAS EN CURSOS NO INSCRITOS'
        WHEN Problema = 2 THEN 'CALIFICACIONES DE PROFESORES NO ASIGNADOS'
        WHEN Problema = 3 THEN 'TAREAS CREADAS POR NO PROFESORES'
        WHEN Problema = 4 THEN 'ENTREGAS SIN CALIFICAR'
        WHEN Problema = 5 THEN 'ESTUDIANTES SIN INSCRIPCIONES'
        WHEN Problema = 6 THEN 'USUARIOS SIN ROLES ASIGNADOS'
        WHEN Problema = 7 THEN 'CURSOS SIN OFERTAS ACTIVAS'
    END as Verificacion,
    COUNT(*) as Problemas,
    CASE 
        WHEN COUNT(*) = 0 THEN 'OK'
        ELSE 'REVISAR'
    END as Estado
FROM (
    -- Problema 1: Estudiantes con entregas en cursos no inscritos
    SELECT 1 as Problema, s.Id
    FROM Submissions s
    JOIN Tasks t ON s.TaskId = t.Id
    LEFT JOIN Enrollments e ON e.StudentId = s.StudentId AND e.CourseOfferingId = t.CourseOfferingId
    WHERE e.Id IS NULL
    
    UNION ALL
    
    -- Problema 2: Calificaciones de profesores no asignados
    SELECT 2, g.Id
    FROM Grades g
    JOIN Submissions s ON g.SubmissionId = s.Id
    JOIN Tasks t ON s.TaskId = t.Id
    JOIN CourseOfferings co ON t.CourseOfferingId = co.Id
    WHERE g.GraderId != co.ProfessorId
    
    UNION ALL
    
    -- Problema 3: Tareas creadas por no profesores
    SELECT 3, t.Id
    FROM Tasks t
    JOIN CourseOfferings co ON t.CourseOfferingId = co.Id
    WHERE t.CreatedBy != co.ProfessorId
    
    UNION ALL
    
    -- Problema 4: Entregas sin calificar
    SELECT 4, s.Id
    FROM Submissions s
    LEFT JOIN Grades g ON s.Id = g.SubmissionId
    WHERE g.Id IS NULL AND s.Status = 'Submitted'
    
    UNION ALL
    
    -- Problema 5: Estudiantes sin inscripciones
    SELECT 5, u.Id
    FROM Users u
    LEFT JOIN Enrollments e ON u.Id = e.StudentId
    WHERE u.UserName LIKE 'est%' AND e.Id IS NULL
    
    UNION ALL
    
    -- Problema 6: Usuarios sin roles asignados
    SELECT 6, u.Id
    FROM Users u
    LEFT JOIN UserRoles ur ON u.Id = ur.UserId
    WHERE ur.Id IS NULL
    
    UNION ALL
    
    -- Problema 7: Cursos sin ofertas activas
    SELECT 7, c.Id
    FROM Courses c
    LEFT JOIN CourseOfferings co ON c.Id = co.CourseId AND co.IsActive = 1
    WHERE co.Id IS NULL AND c.IsActive = 1
) as Problemas
GROUP BY Problema
ORDER BY Problema;

PRINT '';

-- =====================================================
-- 2. DASHBOARD ACADÉMICO COMPLETO
-- =====================================================

PRINT '2. DASHBOARD ACADÉMICO COMPLETO';
PRINT '-----------------------------------';

WITH EstadisticasGenerales AS (
    SELECT 
        COUNT(DISTINCT e.StudentId) as TotalEstudiantes,
        COUNT(DISTINCT co.Id) as TotalCursos,
        COUNT(DISTINCT t.Id) as TotalTareas,
        COUNT(DISTINCT s.Id) as TotalEntregas,
        COUNT(DISTINCT g.Id) as TotalCalificaciones,
        ROUND(AVG(g.Score), 2) as PromedioGeneral,
        (SELECT COUNT(DISTINCT e2.StudentId) * COUNT(DISTINCT t2.Id) 
         FROM CourseOfferings co2 
         LEFT JOIN Enrollments e2 ON co2.Id = e2.CourseOfferingId
         LEFT JOIN Tasks t2 ON co2.Id = t2.CourseOfferingId) as TotalPosiblesEntregas
    FROM CourseOfferings co
    LEFT JOIN Enrollments e ON co.Id = e.CourseOfferingId
    LEFT JOIN Tasks t ON co.Id = t.CourseOfferingId
    LEFT JOIN Submissions s ON t.Id = s.TaskId
    LEFT JOIN Grades g ON s.Id = g.SubmissionId
)
SELECT 
    'RESUMEN GENERAL' as Categoria,
    CONCAT(TotalEstudiantes, ' estudiantes') as Valor
FROM EstadisticasGenerales

UNION ALL

SELECT 
    'CURSOS ACTIVOS',
    CONCAT(TotalCursos, ' cursos')
FROM EstadisticasGenerales

UNION ALL

SELECT 
    'TAREAS ASIGNADAS', 
    CONCAT(TotalTareas, ' tareas')
FROM EstadisticasGenerales

UNION ALL

SELECT 
    'ENTREGAS REALIZADAS',
    CONCAT(TotalEntregas, ' (', 
           ROUND(TotalEntregas * 100.0 / NULLIF(TotalPosiblesEntregas, 0), 2), '%)')
FROM EstadisticasGenerales

UNION ALL

SELECT 
    'CALIFICACIONES',
    CONCAT(TotalCalificaciones, ' (', 
           ROUND(TotalCalificaciones * 100.0 / NULLIF(TotalEntregas, 0), 1), '%)')
FROM EstadisticasGenerales

UNION ALL

SELECT 
    'PROMEDIO GENERAL',
    CONCAT(PromedioGeneral, '/100')
FROM EstadisticasGenerales;

PRINT '';

-- =====================================================
-- 3. ANÁLISIS DETALLADO POR CURSO CON RANKING
-- =====================================================

PRINT '3. ANALISIS DETALLADO POR CURSO';
PRINT '----------------------------------';

WITH CursoStats AS (
    SELECT 
        c.Code as Curso,
        c.Title as NombreCurso,
        p.FullName as Profesor,
        COUNT(DISTINCT e.StudentId) as EstudiantesInscritos,
        COUNT(DISTINCT t.Id) as TareasAsignadas,
        COUNT(DISTINCT s.Id) as EntregasRealizadas,
        COUNT(DISTINCT g.Id) as TareasCalificadas,
        ROUND(AVG(g.Score), 2) as PromedioCurso,
        ROUND(MIN(g.Score), 2) as NotaMinima,
        ROUND(MAX(g.Score), 2) as NotaMaxima,
        ROUND(COUNT(DISTINCT s.Id) * 100.0 / NULLIF((COUNT(DISTINCT e.StudentId) * COUNT(DISTINCT t.Id)), 0), 2) as TasaEntrega,
        ROUND(STDEV(g.Score), 2) as DesviacionEstandar
    FROM Courses c
    JOIN CourseOfferings co ON c.Id = co.CourseId
    JOIN Users p ON co.ProfessorId = p.Id
    LEFT JOIN Enrollments e ON co.Id = e.CourseOfferingId
    LEFT JOIN Tasks t ON co.Id = t.CourseOfferingId
    LEFT JOIN Submissions s ON t.Id = s.TaskId
    LEFT JOIN Grades g ON s.Id = g.SubmissionId
    GROUP BY c.Code, c.Title, p.FullName
)
SELECT 
    Curso,
    NombreCurso,
    Profesor,
    EstudiantesInscritos,
    TareasAsignadas,
    EntregasRealizadas,
    TareasCalificadas,
    CONCAT(TasaEntrega, '%') as TasaEntrega,
    PromedioCurso,
    NotaMinima,
    NotaMaxima,
    DesviacionEstandar,
    CASE 
        WHEN PromedioCurso >= 90 THEN 'EXCELENTE'
        WHEN PromedioCurso >= 80 THEN 'SOBRESALIENTE'
        WHEN PromedioCurso >= 70 THEN 'BUENO'
        ELSE 'REGULAR'
    END as Rendimiento,
    RANK() OVER (ORDER BY PromedioCurso DESC) as Ranking
FROM CursoStats
ORDER BY Ranking;

PRINT '';

-- =====================================================
-- 4. TOP 10 ESTUDIANTES DESTACADOS
-- =====================================================

PRINT '4. TOP 10 ESTUDIANTES DESTACADOS';
PRINT '-----------------------------------';

WITH RendimientoEstudiantes AS (
    SELECT 
        u.Id,
        u.FullName as Estudiante,
        u.UserName,
        COUNT(DISTINCT co.Id) as CursosInscritos,
        COUNT(DISTINCT t.Id) as TareasTotales,
        COUNT(DISTINCT s.Id) as TareasEntregadas,
        ROUND(AVG(g.Score), 2) as PromedioGeneral,
        ROUND(COUNT(DISTINCT s.Id) * 100.0 / NULLIF(COUNT(DISTINCT t.Id), 0), 2) as TasaEntrega,
        COUNT(DISTINCT CASE WHEN g.Score >= 90 THEN g.Id END) as TareasExcelentes,
        COUNT(DISTINCT CASE WHEN g.Score < 70 THEN g.Id END) as TareasBajas
    FROM Users u
    JOIN Enrollments e ON u.Id = e.StudentId
    JOIN CourseOfferings co ON e.CourseOfferingId = co.Id
    LEFT JOIN Tasks t ON co.Id = t.CourseOfferingId
    LEFT JOIN Submissions s ON t.Id = s.TaskId AND s.StudentId = u.Id
    LEFT JOIN Grades g ON s.Id = g.SubmissionId
    WHERE u.UserName LIKE 'est%'
    GROUP BY u.Id, u.FullName, u.UserName
    HAVING COUNT(DISTINCT s.Id) > 0
)
SELECT TOP 10
    Estudiante,
    UserName,
    CursosInscritos,
    TareasTotales,
    TareasEntregadas,
    CONCAT(TasaEntrega, '%') as TasaEntrega,
    PromedioGeneral,
    TareasExcelentes,
    TareasBajas,
    CASE 
        WHEN PromedioGeneral >= 95 THEN 'MEDALLA DE ORO'
        WHEN PromedioGeneral >= 90 THEN 'MEDALLA DE PLATA'
        WHEN PromedioGeneral >= 85 THEN 'MEDALLA DE BRONCE'
        ELSE 'MENCION HONORIFICA'
    END as Reconocimiento,
    RANK() OVER (ORDER BY PromedioGeneral DESC) as RankingGeneral
FROM RendimientoEstudiantes
ORDER BY PromedioGeneral DESC;

PRINT '';

-- =====================================================
-- 5. ANÁLISIS DE PROFESORES Y CARGA ACADÉMICA
-- =====================================================

PRINT '5. ANALISIS DE PROFESORES Y CARGA ACADEMICA';
PRINT '---------------------------------------------';

WITH ProfesorStats AS (
    SELECT 
        u.FullName as Profesor,
        u.Email,
        COUNT(DISTINCT co.Id) as CursosImpartidos,
        COUNT(DISTINCT e.StudentId) as TotalEstudiantes,
        COUNT(DISTINCT t.Id) as TareasCreadas,
        COUNT(DISTINCT s.Id) as EntregasRecibidas,
        COUNT(DISTINCT g.Id) as TareasCalificadas,
        COUNT(DISTINCT a.Id) as AnunciosPublicados,
        ROUND(AVG(g.Score), 2) as PromedioCalificaciones,
        ROUND(COUNT(DISTINCT g.Id) * 100.0 / NULLIF(COUNT(DISTINCT s.Id), 0), 2) as TasaCalificacion
    FROM Users u
    JOIN CourseOfferings co ON u.Id = co.ProfessorId
    LEFT JOIN Enrollments e ON co.Id = e.CourseOfferingId
    LEFT JOIN Tasks t ON co.Id = t.CourseOfferingId
    LEFT JOIN Submissions s ON t.Id = s.TaskId
    LEFT JOIN Grades g ON s.Id = g.SubmissionId AND g.GraderId = u.Id
    LEFT JOIN Announcements a ON co.Id = a.CourseOfferingId AND a.PostedBy = u.Id
    WHERE u.UserName IN ('mgonzalez', 'crodriguez', 'alopez', 'jmartinez')
    GROUP BY u.Id, u.FullName, u.Email
)
SELECT 
    Profesor,
    Email,
    CursosImpartidos,
    TotalEstudiantes,
    TareasCreadas,
    EntregasRecibidas,
    TareasCalificadas,
    CONCAT(TasaCalificacion, '%') as TasaCalificacion,
    AnunciosPublicados,
    PromedioCalificaciones,
    CASE 
        WHEN TotalEstudiantes >= 20 THEN 'ALTA CARGA'
        WHEN TotalEstudiantes >= 10 THEN 'CARGA MEDIA'
        ELSE 'CARGA BAJA'
    END as NivelCarga,
    ROUND(TotalEstudiantes * 1.0 / NULLIF(CursosImpartidos, 0), 1) as EstudiantesPorCurso
FROM ProfesorStats
ORDER BY TotalEstudiantes DESC;

PRINT '';

-- =====================================================
-- 6. DISTRIBUCIÓN DETALLADA DE CALIFICACIONES
-- =====================================================

PRINT '6. DISTRIBUCION DETALLADA DE CALIFICACIONES';
PRINT '----------------------------------------------';

WITH CalificacionesDetalladas AS (
    SELECT 
        CASE 
            WHEN g.Score >= 95 THEN '95-100: SOBRESALIENTE'
            WHEN g.Score >= 90 THEN '90-94: EXCELENTE'
            WHEN g.Score >= 85 THEN '85-89: MUY BUENO'
            WHEN g.Score >= 80 THEN '80-84: BUENO'
            WHEN g.Score >= 75 THEN '75-79: SATISFACTORIO'
            WHEN g.Score >= 70 THEN '70-74: REGULAR'
            WHEN g.Score >= 60 THEN '60-69: NECESITA MEJORAR'
            ELSE '0-59: INSUFICIENTE'
        END as Categoria,
        g.Score
    FROM Grades g
)
SELECT 
    Categoria,
    COUNT(*) as Cantidad,
    ROUND(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM Grades), 2) as Porcentaje,
    REPLICATE('*', COUNT(*) / 5) as Histograma,
    ROUND(AVG(Score), 1) as PromedioRango,
    MIN(Score) as Minimo,
    MAX(Score) as Maximo
FROM CalificacionesDetalladas
GROUP BY Categoria
ORDER BY MIN(Score) DESC;

PRINT '';

-- =====================================================
-- 7. ANÁLISIS TEMPORAL AVANZADO
-- =====================================================

PRINT '7. ANALISIS TEMPORAL AVANZADO';
PRINT '--------------------------------';

WITH ActividadMensual AS (
    SELECT 
        FORMAT(s.SubmittedAt, 'yyyy-MM') as Mes,
        DATENAME(MONTH, s.SubmittedAt) + ' ' + DATENAME(YEAR, s.SubmittedAt) as MesCompleto,
        COUNT(s.Id) as EntregasRealizadas,
        COUNT(g.Id) as TareasCalificadas,
        ROUND(AVG(g.Score), 2) as PromedioMensual,
        COUNT(DISTINCT s.StudentId) as EstudiantesActivos,
        ROUND(COUNT(g.Id) * 100.0 / NULLIF(COUNT(s.Id), 0), 2) as TasaCalificacionMensual
    FROM Submissions s
    LEFT JOIN Grades g ON s.Id = g.SubmissionId
    WHERE s.SubmittedAt IS NOT NULL
    GROUP BY FORMAT(s.SubmittedAt, 'yyyy-MM'), DATENAME(MONTH, s.SubmittedAt) + ' ' + DATENAME(YEAR, s.SubmittedAt)
)
SELECT 
    MesCompleto,
    EntregasRealizadas,
    TareasCalificadas,
    CONCAT(TasaCalificacionMensual, '%') as TasaCalificacion,
    EstudiantesActivos,
    PromedioMensual,
    CASE 
        WHEN EntregasRealizadas > LAG(EntregasRealizadas) OVER (ORDER BY Mes) THEN 'ASCENDENTE'
        WHEN EntregasRealizadas < LAG(EntregasRealizadas) OVER (ORDER BY Mes) THEN 'DESCENDENTE'
        ELSE 'ESTABLE'
    END as Tendencia,
    ROUND((EntregasRealizadas - LAG(EntregasRealizadas) OVER (ORDER BY Mes)) * 100.0 / NULLIF(LAG(EntregasRealizadas) OVER (ORDER BY Mes), 0), 2) as CrecimientoPorcentual
FROM ActividadMensual
ORDER BY Mes;

PRINT '';

-- =====================================================
-- 8. COMPARATIVA ENTREGAS VS CALIFICACIONES POR TAREA
-- =====================================================

PRINT '8. COMPARATIVA ENTREGAS VS CALIFICACIONES';
PRINT '-------------------------------------------';

SELECT 
    c.Code as Curso,
    t.Title as Tarea,
    FORMAT(t.DueDate, 'dd/MM/yyyy') as FechaLimite,
    COUNT(DISTINCT e.StudentId) as EstudiantesInscritos,
    COUNT(s.Id) as EntregasRealizadas,
    COUNT(g.Id) as TareasCalificadas,
    ROUND(COUNT(s.Id) * 100.0 / NULLIF(COUNT(DISTINCT e.StudentId), 0), 2) as PorcentajeEntrega,
    ROUND(COUNT(g.Id) * 100.0 / NULLIF(COUNT(s.Id), 0), 2) as PorcentajeCalificacion,
    ROUND(AVG(g.Score), 2) as PromedioTarea,
    CASE 
        WHEN COUNT(g.Id) = COUNT(s.Id) THEN 'COMPLETAMENTE CALIFICADA'
        WHEN COUNT(g.Id) = 0 THEN 'SIN CALIFICAR'
        ELSE 'CALIFICACION PARCIAL'
    END as EstadoCalificacion,
    CASE 
        WHEN COUNT(s.Id) = COUNT(DISTINCT e.StudentId) THEN '100% ENTREGA'
        WHEN COUNT(s.Id) >= COUNT(DISTINCT e.StudentId) * 0.8 THEN 'ALTA ENTREGA'
        WHEN COUNT(s.Id) >= COUNT(DISTINCT e.StudentId) * 0.6 THEN 'MEDIA ENTREGA'
        ELSE 'BAJA ENTREGA'
    END as NivelEntrega
FROM Tasks t
JOIN CourseOfferings co ON t.CourseOfferingId = co.Id
JOIN Courses c ON co.CourseId = c.Id
LEFT JOIN Enrollments e ON co.Id = e.CourseOfferingId
LEFT JOIN Submissions s ON t.Id = s.TaskId
LEFT JOIN Grades g ON s.Id = g.SubmissionId
GROUP BY c.Code, t.Title, t.DueDate, t.Id
ORDER BY c.Code, t.DueDate;

PRINT '';

-- =====================================================
-- 9. DETECCIÓN DE PATRONES Y ANOMALÍAS
-- =====================================================

PRINT '9. DETECCION DE PATRONES Y ANOMALIAS';
PRINT '---------------------------------------';

-- Patrón 1: Diferencia entre cursos
WITH PromediosCursos AS (
    SELECT 
        c.Code as Curso,
        AVG(g.Score) as PromedioCurso
    FROM Courses c
    JOIN CourseOfferings co ON c.Id = co.CourseId
    JOIN Tasks t ON co.Id = t.CourseOfferingId
    JOIN Submissions s ON t.Id = s.TaskId
    JOIN Grades g ON s.Id = g.SubmissionId
    GROUP BY c.Code
),
CursoMaxMin AS (
    SELECT 
        MAX(PromedioCurso) as promedio_max,
        MIN(PromedioCurso) as promedio_min
    FROM PromediosCursos
),
CursosExtremos AS (
    SELECT 
        (SELECT TOP 1 Curso FROM PromediosCursos WHERE PromedioCurso = (SELECT promedio_max FROM CursoMaxMin)) as curso_max,
        (SELECT TOP 1 Curso FROM PromediosCursos WHERE PromedioCurso = (SELECT promedio_min FROM CursoMaxMin)) as curso_min,
        (SELECT promedio_max FROM CursoMaxMin) as promedio_max,
        (SELECT promedio_min FROM CursoMaxMin) as promedio_min
)
SELECT 
    'PATRON: DIFERENCIA ENTRE CURSOS' as Tipo,
    CONCAT('Mayor diferencia: ', ROUND(promedio_max - promedio_min, 2), ' puntos') as Descripcion,
    CONCAT('Curso mas alto: ', curso_max, ' (', ROUND(promedio_max, 2), ')') as Detalle1,
    CONCAT('Curso mas bajo: ', curso_min, ' (', ROUND(promedio_min, 2), ')') as Detalle2
FROM CursosExtremos;

-- Patrón 2: Estudiante destacado
WITH PromediosEstudiantes AS (
    SELECT 
        u.Id,
        u.FullName as Estudiante,
        AVG(g.Score) as PromedioGeneral
    FROM Users u
    JOIN Submissions s ON u.Id = s.StudentId
    JOIN Grades g ON s.Id = g.SubmissionId
    WHERE u.UserName LIKE 'est%'
    GROUP BY u.Id, u.FullName
),
EstudianteDestacado AS (
    SELECT TOP 1
        Estudiante,
        PromedioGeneral
    FROM PromediosEstudiantes
    ORDER BY PromedioGeneral DESC
)
SELECT 
    'PATRON: ESTUDIANTE DESTACADO' as Tipo,
    'Mayor promedio general' as Descripcion,
    CONCAT('Estudiante: ', Estudiante) as Detalle1,
    CONCAT('Promedio: ', ROUND(PromedioGeneral, 2)) as Detalle2
FROM EstudianteDestacado;

-- Patrón 3: Entregas puntuales
WITH TasasEntrega AS (
    SELECT 
        COUNT(s.Id) as total_entregas,
        (SELECT COUNT(*) FROM Enrollments e2 
         JOIN Tasks t2 ON e2.CourseOfferingId = t2.CourseOfferingId) as total_posibles_entregas
    FROM Submissions s
)
SELECT 
    'PATRON: ENTREGAS PUNTUALES' as Tipo,
    'Tasa de entrega global' as Descripcion,
    CONCAT('Porcentaje: ', ROUND(total_entregas * 100.0 / NULLIF(total_posibles_entregas, 0), 2), '%') as Detalle1,
    CONCAT('Total entregas: ', total_entregas) as Detalle2
FROM TasasEntrega;

PRINT '';

-- =====================================================
-- 10. ESTADÍSTICAS ACADÉMICAS DETALLADAS
-- =====================================================

PRINT '10. ESTADISTICAS ACADEMICAS DETALLADAS';
PRINT '---------------------------------------';

SELECT 
    c.Code as Curso,
    c.Title as NombreCurso,
    COUNT(DISTINCT e.StudentId) as EstudiantesInscritos,
    COUNT(DISTINCT t.Id) as TareasTotales,
    COUNT(DISTINCT s.Id) as EntregasRealizadas,
    COUNT(DISTINCT g.Id) as TareasCalificadas,
    ROUND(AVG(g.Score), 2) as PromedioCurso,
    ROUND(MIN(g.Score), 2) as NotaMinima,
    ROUND(MAX(g.Score), 2) as NotaMaxima,
    ROUND(COUNT(DISTINCT s.Id) * 100.0 / NULLIF(COUNT(DISTINCT e.StudentId) * COUNT(DISTINCT t.Id), 0), 2) as TasaEntrega
FROM Courses c
JOIN CourseOfferings co ON c.Id = co.CourseId
LEFT JOIN Enrollments e ON co.Id = e.CourseOfferingId
LEFT JOIN Tasks t ON co.Id = t.CourseOfferingId
LEFT JOIN Submissions s ON t.Id = s.TaskId
LEFT JOIN Grades g ON s.Id = g.SubmissionId
GROUP BY c.Code, c.Title
ORDER BY PromedioCurso DESC;

PRINT '';

-- =====================================================
-- 11. DESEMPEÑO DE ESTUDIANTES POR CURSO
-- =====================================================

PRINT '11. DESEMPEÑO DE ESTUDIANTES POR CURSO';
PRINT '---------------------------------------';

SELECT 
    u.FullName as Estudiante,
    c.Code as Curso,
    c.Title as NombreCurso,
    COUNT(t.Id) as TareasAsignadas,
    COUNT(s.Id) as TareasEntregadas,
    COUNT(g.Id) as TareasCalificadas,
    ROUND(AVG(g.Score), 2) as PromedioEstudiante,
    CASE 
        WHEN AVG(g.Score) >= 90 THEN 'Excelente'
        WHEN AVG(g.Score) >= 80 THEN 'Bueno'
        WHEN AVG(g.Score) >= 70 THEN 'Aceptable'
        WHEN AVG(g.Score) >= 60 THEN 'Necesita Mejorar'
        ELSE 'Bajo Rendimiento'
    END as Categoria
FROM Users u
JOIN Enrollments e ON u.Id = e.StudentId
JOIN CourseOfferings co ON e.CourseOfferingId = co.Id
JOIN Courses c ON co.CourseId = c.Id
LEFT JOIN Tasks t ON co.Id = t.CourseOfferingId
LEFT JOIN Submissions s ON t.Id = s.TaskId AND s.StudentId = u.Id
LEFT JOIN Grades g ON s.Id = g.SubmissionId
WHERE u.UserName LIKE 'est%'
GROUP BY u.Id, u.FullName, c.Code, c.Title
ORDER BY u.FullName, c.Code;

PRINT '';

-- =====================================================
-- 12. ANÁLISIS DE ENTREGAS POR TAREA
-- =====================================================

PRINT '12. ANALISIS DE ENTREGAS POR TAREA';
PRINT '-----------------------------------';

SELECT 
    c.Code as Curso,
    t.Title as Tarea,
    t.DueDate as FechaLimite,
    COUNT(DISTINCT e.StudentId) as EstudiantesInscritos,
    COUNT(s.Id) as EntregasRealizadas,
    COUNT(g.Id) as TareasCalificadas,
    ROUND(COUNT(s.Id) * 100.0 / NULLIF(COUNT(DISTINCT e.StudentId), 0), 2) as PorcentajeEntrega,
    ROUND(AVG(g.Score), 2) as PromedioTarea
FROM Tasks t
JOIN CourseOfferings co ON t.CourseOfferingId = co.Id
JOIN Courses c ON co.CourseId = c.Id
LEFT JOIN Enrollments e ON co.Id = e.CourseOfferingId
LEFT JOIN Submissions s ON t.Id = s.TaskId
LEFT JOIN Grades g ON s.Id = g.SubmissionId
GROUP BY c.Code, t.Title, t.DueDate, t.Id
ORDER BY c.Code, t.DueDate;

PRINT '';

-- =====================================================
-- 13. ESTUDIANTES CON RENDIMIENTO DESTACADO Y BAJO
-- =====================================================

PRINT '13. ESTUDIANTES DESTACADOS Y CON BAJO RENDIMIENTO';
PRINT '--------------------------------------------------';

WITH RendimientoEstudiantes AS (
    SELECT 
        u.Id,
        u.FullName as Estudiante,
        COUNT(DISTINCT co.Id) as CursosInscritos,
        COUNT(DISTINCT t.Id) as TareasTotales,
        COUNT(DISTINCT s.Id) as TareasEntregadas,
        ROUND(AVG(g.Score), 2) as PromedioGeneral,
        ROUND(COUNT(DISTINCT s.Id) * 100.0 / NULLIF(COUNT(DISTINCT t.Id), 0), 2) as TasaEntrega
    FROM Users u
    JOIN Enrollments e ON u.Id = e.StudentId
    JOIN CourseOfferings co ON e.CourseOfferingId = co.Id
    LEFT JOIN Tasks t ON co.Id = t.CourseOfferingId
    LEFT JOIN Submissions s ON t.Id = s.TaskId AND s.StudentId = u.Id
    LEFT JOIN Grades g ON s.Id = g.SubmissionId
    WHERE u.UserName LIKE 'est%'
    GROUP BY u.Id, u.FullName
)
SELECT 
    Estudiante,
    CursosInscritos,
    TareasTotales,
    TareasEntregadas,
    TasaEntrega,
    PromedioGeneral,
    CASE 
        WHEN PromedioGeneral >= 90 AND TasaEntrega >= 90 THEN 'DESTACADO'
        WHEN PromedioGeneral < 60 OR TasaEntrega < 50 THEN 'NECESITA ATENCION'
        ELSE 'REGULAR'
    END as Estado
FROM RendimientoEstudiantes
ORDER BY PromedioGeneral DESC;

PRINT '';

-- =====================================================
-- 14. ANÁLISIS TEMPORAL DE ACTIVIDAD
-- =====================================================

PRINT '14. ANALISIS TEMPORAL DE ACTIVIDAD';
PRINT '-----------------------------------';

SELECT 
    CONVERT(VARCHAR(7), s.SubmittedAt, 120) as Mes,
    COUNT(s.Id) as EntregasRealizadas,
    COUNT(g.Id) as TareasCalificadas,
    ROUND(AVG(g.Score), 2) as PromedioMensual
FROM Submissions s
LEFT JOIN Grades g ON s.Id = g.SubmissionId
WHERE s.SubmittedAt IS NOT NULL
GROUP BY CONVERT(VARCHAR(7), s.SubmittedAt, 120)
ORDER BY Mes;

PRINT '';

-- =====================================================
-- 15. VERIFICACIÓN COMPLETA DE DATOS POR TABLA
-- =====================================================

PRINT '15. VERIFICACION COMPLETA POR TABLA';
PRINT '------------------------------------';

SELECT 'Roles' as Tabla, COUNT(*) as Registros FROM Roles
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
ORDER BY Registros DESC;

PRINT '';

-- =====================================================
-- 16. DETALLE COMPLETO DE RELACIONES
-- =====================================================

PRINT '16. DETALLE COMPLETO DE RELACIONES';
PRINT '-----------------------------------';

SELECT 
    c.Code as Curso,
    c.Title as NombreCurso,
    co.Section as Seccion,
    p.FullName as Profesor,
    COUNT(DISTINCT e.StudentId) as EstudiantesInscritos,
    COUNT(DISTINCT t.Id) as TareasCreadas,
    COUNT(DISTINCT a.Id) as AnunciosPublicados
FROM Courses c
JOIN CourseOfferings co ON c.Id = co.CourseId
JOIN Users p ON co.ProfessorId = p.Id
LEFT JOIN Enrollments e ON co.Id = e.CourseOfferingId
LEFT JOIN Tasks t ON co.Id = t.CourseOfferingId
LEFT JOIN Announcements a ON co.Id = a.CourseOfferingId
GROUP BY c.Code, c.Title, co.Section, p.FullName
ORDER BY c.Code, co.Section;

PRINT '';

-- =====================================================
-- 17. RESUMEN EJECUTIVO FINAL
-- =====================================================

PRINT '17. RESUMEN EJECUTIVO FINAL';
PRINT '------------------------------';

SELECT 
    Tabla,
    Registros,
    CASE 
        WHEN Tabla = 'Roles' AND Registros = 3 THEN 'OPTIMO'
        WHEN Tabla = 'Users' AND Registros = 13 THEN 'OPTIMO'
        WHEN Tabla = 'UserRoles' AND Registros = 13 THEN 'OPTIMO'
        WHEN Tabla = 'Courses' AND Registros = 6 THEN 'OPTIMO'
        WHEN Tabla = 'CourseOfferings' AND Registros = 6 THEN 'OPTIMO'
        WHEN Tabla = 'Enrollments' AND Registros = 48 THEN 'OPTIMO'
        WHEN Tabla = 'Tasks' AND Registros = 18 THEN 'OPTIMO'
        WHEN Tabla = 'Submissions' AND Registros = 144 THEN 'OPTIMO'
        WHEN Tabla = 'Grades' AND Registros = 144 THEN 'OPTIMO'
        WHEN Tabla = 'Announcements' AND Registros >= 5 THEN 'OPTIMO'
        ELSE 'REVISAR'
    END as Estado
FROM (
    SELECT 'Roles' as Tabla, COUNT(*) as Registros FROM Roles
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
) as Resumen
ORDER BY 
    CASE 
        WHEN Tabla IN ('Roles', 'Users', 'Courses') THEN 1
        WHEN Tabla IN ('Enrollments', 'Tasks', 'Submissions') THEN 2
        ELSE 3
    END, Tabla;

PRINT '';
PRINT '===================================================================';
PRINT 'VERIFICACION COMPLETADA - TODOS LOS ANALISIS FINALIZADOS';
PRINT '===================================================================';
PRINT '';
PRINT 'RESUMEN DE ACCIONES RECOMENDADAS:';
PRINT '1. Revisar integridad referencial (Seccion 1)';
PRINT '2. Analizar estudiantes destacados (Seccion 4)';
PRINT '3. Verificar carga academica de profesores (Seccion 5)';
PRINT '4. Monitorear distribucion de calificaciones (Seccion 6)';
PRINT '5. Identificar patrones y anomalias (Seccion 9)';
PRINT '6. Revisar desempeno por estudiante (Seccion 11)';
PRINT '7. Analizar entregas por tarea (Seccion 12)';
PRINT '8. Verificar estudiantes con bajo rendimiento (Seccion 13)';
PRINT '';

-- Verificación final del estado del sistema
DECLARE @Problemas INT;
SELECT @Problemas = COUNT(*) 
FROM (
    SELECT s.Id as ProblemaId
    FROM Submissions s
    JOIN Tasks t ON s.TaskId = t.Id
    LEFT JOIN Enrollments e ON e.StudentId = s.StudentId AND e.CourseOfferingId = t.CourseOfferingId
    WHERE e.Id IS NULL
) as problemas;

PRINT 'ESTADO DEL SISTEMA: ' + 
    CASE WHEN @Problemas > 0 THEN 'REQUIERE ATENCION' ELSE 'OPTIMO' END;
PRINT '===================================================================';