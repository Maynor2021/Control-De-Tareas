# Sistema de Autorización por Roles - Implementado

## ✅ Tareas Completadas

### 1. Configurar políticas de autorización en Program.cs
- ✓ Agregada autenticación por cookies
- ✓ Configuradas políticas Admin, Profesor, Estudiante

### 2. Crear atributos [Authorize] personalizados por rol
- ✓ `AdminAuthorize` 
- ✓ `ProfesorAuthorize`
- ✓ `EstudianteAuthorize`
- ✓ `ProfesorOAdminAuthorize`

### 3. Implementar autorización en controllers clave
- ✓ `TareasController` - Requiere autenticación
- ✓ `CursosController` - Solo profesores y admin
- ✓ `AccountController` - Login/Logout

### 4. Crear páginas de error 403 (Acceso Denegado)
- ✓ `ErrorController` creado
- ✓ Vista `Error403.cshtml` creada

### 5. Implementar redirecciones según permisos
- ✓ Configurado en Program.cs
- ✓ Redirige a `/Error/403` cuando no tiene permisos

### 6. Validar acceso a nivel de vista
- ✓ `_Sidebar.cshtml` - Filtra menú por rol
- ✓ `_Layout.cshtml` - Muestra badge de rol

---

## 🔐 Roles y Permisos

- **Administrador:** Acceso completo
- **Profesor:** Gestión de cursos y tareas
- **Estudiante:** Ver cursos y entregar tareas

---

## 📋 Criterios de Aceptación

✓ Roles aplicados correctamente
✓ Usuarios no autorizados no acceden a recursos
✓ Redirección apropiada en caso de acceso denegado
✓ Menús muestran solo opciones autorizadas
✓ Políticas de autorización funcionando

---

## 🗄️ Script SQL

Ejecutar: `Scripts_SQL_Usuarios_Prueba.sql`

Credenciales:
- admin@test.com / 123456
- profesor@test.com / 123456
- estudiante@test.com / 123456
