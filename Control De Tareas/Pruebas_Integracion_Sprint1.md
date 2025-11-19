# 🧪 Pruebas de Integración – Sprint 1  
Control de Tareas – ASP.NET Core 8

Este documento describe las pruebas ejecutadas para validar el flujo completo de autenticación, autorización y navegación del sistema.

---

## 1️⃣ Registro de Usuario

### ✔ Registro exitoso
- **Acción:** Registrar nuevo usuario con datos válidos  
- **Resultado:** ✔ Aprobado  
- **Observación:** El sistema crea el usuario correctamente.

### ✔ Validación de campos requeridos
- **Acción:** Enviar formulario vacío o incompleto  
- **Resultado:** ✔ Aprobado  
- **Observación:** El formulario muestra mensajes de validación.

### ✔ Prevención de duplicados
- **Acción:** Registrar usuario con email existente  
- **Resultado:** ✔ Aprobado  
- **Observación:** Se muestra mensaje “correo ya existe”.

---

## 2️⃣ Login / Logout

### ✔ Login correcto
- **Acción:** Ingresar credenciales válidas  
- **Resultado:** ✔ Aprobado  
- **Observación:** Redirige según el rol.

### ✔ Login incorrecto
- **Acción:** Ingresar credenciales inválidas  
- **Resultado:** ✔ Aprobado  
- **Observación:** Muestra "Error al iniciar sesión".

### ✔ Logout
- **Acción:** Cerrar sesión  
- **Resultado:** ✔ Aprobado  
- **Observación:** Elimina cookie y vuelve al login.

---

## 3️⃣ Autorización

### ✔ Acceso correcto según rol
- **Prueba:**  
  - Admin accede a dashboard admin  
  - Profesor accede a dashboard profesor  
  - Estudiante accede a dashboard estudiante  
- **Resultado:** ✔ Aprobado

### ✔ Bloqueo de acceso no autorizado
- **Prueba:** Estudiante intenta entrar al dashboard admin  
- **Resultado:** ✔ Aprobado  
- **Observación:** Se redirige al login o acceso denegado.

### ✔ Redirección si no está autenticado
- **Prueba:** Entrar a `/Dashboard/Admin` sin login  
- **Resultado:** ✔ Aprobado  

---

## 4️⃣ Navegación

### ✔ Dashboard según rol
- **Resultado:** ✔ Aprobado

### ✔ Menú dinámico según rol
- **Resultado:** ✔ Aprobado  
- **Observación:** El menú muestra solo las opciones del rol.

---

## 5️⃣ Conclusión

✔ Todas las pruebas del Sprint 1 fueron ejecutadas  
✔ No se encontraron errores críticos  
✔ El flujo de autenticación funciona correctamente  
✔ Los dashboards por rol funcionan de forma estable  

**Sprint validado y listo para integración.**

---
