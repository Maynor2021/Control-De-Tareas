# ⚙️ SETUP – Guía de Instalación y Configuración

Este documento explica cómo instalar, configurar y ejecutar el proyecto **Control de Tareas** en un entorno local.

---

# 1. Requisitos previos

- **Windows 10/11**
- **Visual Studio 2022** (v17.8 o superior)
- **.NET SDK 8.0**
- **SQL Server 2019 o 2022**
- **SSMS** (SQL Server Management Studio)
- **Git**

---

# 2. Clonar el repositorio

```bash
git clone https://github.com/Maynor2021/Control-De-Tareas.git
```

Abrir:

```
Control De Tareas.sln
```

---

# 3. Configuración de la Base de Datos

Editar el archivo:

```
appsettings.json
```

Con tu conexión local:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR\\INSTANCIA;Database=ControlDeTareasDB;User Id=USUARIO;Password=PASSWORD;TrustServerCertificate=true"
}
```

📌 **Este archivo es local y NO debe subirse al repositorio.**

---

# 4. Ejecutar Migraciones

Abrir **Package Manager Console**:

```bash
Update-Database
```

Esto crea las tablas:

- Users  
- Roles  
- UserRoles  
- Courses  
- Tareas  
- Submissions  

---

# 5. Estructura del Proyecto

```
Controllers/
    HomeController.cs
    CursosController.cs
    TareasController.cs

Models/
    CursosVm.cs
    TareasVm.cs
    MenuItem.cs

Data/
    Entitys/
        Users.cs
        Roles.cs
        UserRoles.cs
        Courses.cs
        Tareas.cs
        Submissions.cs

    Configurations/
        UsersConfig.cs
        RolesConfig.cs
        CoursesConfig.cs
        TareasConfig.cs
        UserRolesConfig.cs
        SubmissionsConfig.cs

Services/
    MenuServices.cs
```

---

# 6. Variables de entorno (opcional)

```
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=<cadena segura>
```

---

# 7. Troubleshooting

### ❌ Error: "Login failed for user"
✔ Verifica usuario y contraseña SQL.

### ❌ Error: "Cannot open database"
✔ Ejecuta `Update-Database`.

### ❌ Error: "sp_getapplock" en migraciones
✔ Asegúrate de que SQL Server esté iniciado.

### ❌ Error por appsettings.json del compañero
✔ Cada dev debe tener su propia cadena de conexión.

---

# 8. Comentarios en Código

Se documentaron las clases principales:

- Controladores  
- Entidades  
- ViewModels  
- Servicios  
- Configuraciones EF Core  
- Context  

Documentación agregada con **XML Comments**.

---

# 9. Onboarding

Un nuevo desarrollador puede:

1. Clonar el repo  
2. Configurar su conexión local  
3. Ejecutar `Update-Database`  
4. Ejecutar el proyecto  
5. Iniciar sesión según el rol asignado  

---

# ✔ Proyecto listo para desarrollo colaborativo
