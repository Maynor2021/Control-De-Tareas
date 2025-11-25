# 📘 Plataforma de Control de Tareas – ASP.NET Core 8

Aplicación web desarrollada en **ASP.NET Core 8 (MVC)** para la gestión de cursos, tareas, roles, entregas y calificaciones.  
Incluye autenticación por roles, navegación dinámica y administración educativa básica.  
Proyecto académico colaborativo desarrollado en equipo.

---

## 🧩 Funcionalidades principales

### 👤 Autenticación y Roles
- Login con Claims + Cookies  
- Logout con limpieza de sesión  
- Roles:
  - Administrador  
  - Profesor  
  - Estudiante  
- Atributos personalizados de autorización:
  - `[ProfesorAuthorize]`
  - `[EstudianteAuthorize]`
  - `[ProfesorOAdminAuthorize]`

### 📚 Gestión Académica
- Cursos  
- Tareas  
- Entregas de estudiantes (Submissions)  
- Calificaciones  

### 🧭 Menú dinámico según rol
Renderizado desde `MenuServices` y `MenuItem`, mostrando solo las opciones permitidas.

### 💾 Base de Datos
Tablas incluidas:
- Users  
- Roles  
- UserRoles  
- Courses  
- Tareas  
- Submissions  

Configuraciones dentro de:

```
Data/Configurations/
```

---

## 🛠 Tecnologías utilizadas

- **ASP.NET Core 8 (MVC)**
- **C# 12**
- **Entity Framework Core 9**
- **SQL Server 2019+**
- **Bootstrap 5**
- **jQuery**
- **Git & GitHub**
- **EF Migrations**

---

## ⚙️ Instalación del proyecto

### 1️⃣ Clonar el repositorio

```bash
git clone https://github.com/Maynor2021/Control-De-Tareas.git
```

### 2️⃣ Abrir en Visual Studio

Archivo a abrir:

```
Control De Tareas.sln
```

### 3️⃣ Configurar cadena de conexión

Editar tu `appsettings.json` (local, NO subirlo):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR\\INSTANCIA;Database=ControlDeTareasDB;User Id=USUARIO;Password=TU_PASSWORD;TrustServerCertificate=true"
}
```

📌 *Cada desarrollador usa su propia conexión.*  
📌 **No subir appsettings.json al repositorio.**

---

## 🗄️ Migraciones (EF Core)

### Aplicar migraciones existentes:
```bash
Update-Database
```

### Crear una nueva migración:
```bash
Add-Migration NombreDeMigracion
```

### Revertir última migración:
```bash
Remove-Migration
```

---

# 👥 **Usuarios de prueba del sistema**

A continuación, las credenciales utilizadas en el proyecto:

---

## 🟥 **Administrador**
- **Correo:** `admin@sistema.com`  
- **Contraseña:** `admin123`

---

## 🟦 **Profesores**

- `maria.gonzalez@sistema.com` — **admin123**  
- `carlos.rodriguez@sistema.com` — **admin123!**  
- `ana.lopez@sistema.com` — **admin123!**  
- `jose.martinez@sistema.com` — **admin123!**

---

## 🟩 **Estudiantes**

- `ana.martinez@sistema.com` — **admin123**  
- `luis.hernandez@sistema.com` — **admin123**  
- `sofia.ramirez@sistema.com` — **admin123**  
- `carlos.garcia@sistema.com` — **admin123**  
- `marta.lopez@sistema.com` — **admin123**  
- `pedro.sanchez@sistema.com` — **admin123**  
- `laura.diaz@sistema.com` — **admin123**  
- `david.torres@sistema.com` — **admin123**

---

## 🧑‍💻 Equipo de desarrollo
Proyecto grupal del curso **Desarrollo de Aplicaciones Web – Sprint 1**.

---

## 📄 Licencia
Proyecto académico — uso interno del equipo.

