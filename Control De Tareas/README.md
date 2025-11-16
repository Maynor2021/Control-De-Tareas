# 📘 Plataforma de Control de Tareas – ASP.NET Core 8

Aplicación web desarrollada en **ASP.NET Core 8 (MVC)** para la gestión de cursos, tareas, roles, entregas y calificaciones.  
Es un proyecto académico desarrollado por un equipo, con soporte para profesor, estudiante y administrador.

---

## 🧩 Funcionalidades principales

### 👤 Autenticación y Roles  
- Login con Claims y Cookies  
- Roles: **admin, profesor, estudiante**  
- Autorización mediante atributos personalizados:
  - `[ProfesorAuthorize]`
  - `[EstudianteAuthorize]`
  - `[ProfesorOAdminAuthorize]`

### 📚 Gestión Académica
- Cursos
- Tareas
- Entregas (Submissions)
- Calificaciones

### 🧭 Menú dinámico (según rol)
Administrado por `MenuServices` y `MenuItem`.

### 💾 Base de Datos
- Users  
- Roles  
- UserRoles  
- Courses  
- Tareas  
- Submissions  

Usando **Entity Framework Core 9** con configuraciones individuales en la carpeta:
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
- **EF Migrations**
- **Git & GitHub**

---

## ⚙️ Instalación del proyecto

### 1️⃣ Clonar el repositorio
```bash
git clone https://github.com/Maynor2021/Control-De-Tareas.git
```

### 2️⃣ Abrir el proyecto
Abrir en Visual Studio 2022:

```
Control De Tareas.sln
```

### 3️⃣ Configurar la base de datos
Editar `appsettings.json` con tu propio servidor local:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR\\INSTANCIA;Database=ControlDeTareasDB;User Id=USUARIO;Password=TU_PASSWORD;TrustServerCertificate=true"
}
```

📌 *Nota:* Cada desarrollador usa su propia cadena de conexión.  
**NO se debe subir appsettings.json al repositorio.**

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

## 👥 Usuarios de prueba (placeholder)

Pendiente de definir por el equipo. Ejemplo:

```
Administrador:
Email: admin@demo.com
Password: Admin123

Profesor:
Email: profesor@demo.com
Password: Profe123

Estudiante:
Email: estudiante@demo.com
Password: Estu123
```

---

## 🧑‍💻 Equipo de desarrollo
Proyecto grupal del curso **Desarrollo de Aplicaciones Web – Sprint 1**.

---

## 📄 Licencia
Proyecto académico. Uso interno del equipo.
