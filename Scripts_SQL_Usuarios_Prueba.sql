-- ============================================
-- SCRIPT: Usuarios de Prueba con Roles
-- Descripción: Inserta usuarios de ejemplo para probar el sistema de autorización
-- ============================================

-- 1. Insertar Roles (si no existen)
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Administrador')
BEGIN
    INSERT INTO Roles (RoleId, RoleName, Description, CreateAt, IsSoftDeleted)
    VALUES (NEWID(), 'Administrador', 'Acceso completo al sistema', GETDATE(), 0);
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Profesor')
BEGIN
    INSERT INTO Roles (RoleId, RoleName, Description, CreateAt, IsSoftDeleted)
    VALUES (NEWID(), 'Profesor', 'Gestión de cursos y tareas', GETDATE(), 0);
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Estudiante')
BEGIN
    INSERT INTO Roles (RoleId, RoleName, Description, CreateAt, IsSoftDeleted)
    VALUES (NEWID(), 'Estudiante', 'Ver y entregar tareas', GETDATE(), 0);
END

-- 2. Insertar Usuarios de Prueba
DECLARE @AdminId UNIQUEIDENTIFIER = NEWID()
DECLARE @ProfesorId UNIQUEIDENTIFIER = NEWID()
DECLARE @EstudianteId UNIQUEIDENTIFIER = NEWID()

-- Administrador
INSERT INTO Users (UserId, UserName, Instructor, Email, PasswordHash, CreateAt, IsSoftDeleted)
VALUES (@AdminId, 'Admin', 'Carlos Administrador', 'admin@test.com', '123456', GETDATE(), 0);

-- Profesor
INSERT INTO Users (UserId, UserName, Instructor, Email, PasswordHash, CreateAt, IsSoftDeleted)
VALUES (@ProfesorId, 'Profesor', 'Juan Profesor', 'profesor@test.com', '123456', GETDATE(), 0);

-- Estudiante
INSERT INTO Users (UserId, UserName, Instructor, Email, PasswordHash, CreateAt, IsSoftDeleted)
VALUES (@EstudianteId, 'Estudiante', 'María Estudiante', 'estudiante@test.com', '123456', GETDATE(), 0);

-- 3. Asignar Roles a Usuarios
INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
VALUES 
    (@AdminId, (SELECT RoleId FROM Roles WHERE RoleName = 'Administrador'), GETDATE()),
    (@ProfesorId, (SELECT RoleId FROM Roles WHERE RoleName = 'Profesor'), GETDATE()),
    (@EstudianteId, (SELECT RoleId FROM Roles WHERE RoleName = 'Estudiante'), GETDATE());

-- 4. Verificar datos insertados
SELECT 
    u.UserName,
    u.Email,
    r.RoleName,
    u.CreateAt
FROM Users u
INNER JOIN UserRoles ur ON u.UserId = ur.UserId
INNER JOIN Roles r ON ur.RoleId = r.RoleId
WHERE u.IsSoftDeleted = 0;

-- ============================================
-- CREDENCIALES DE PRUEBA:
-- ============================================
-- Administrador:
--   Email: admin@test.com
--   Password: 123456

-- Profesor:
--   Email: profesor@test.com
--   Password: 123456

-- Estudiante:
--   Email: estudiante@test.com
--   Password: 123456
-- ============================================
