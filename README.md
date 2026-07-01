# PEIA — Plataforma Empresarial Inteligente para Almacenes

PEIA es un monolito modular basado en **.NET 8** y **PostgreSQL**, diseñado para gestionar bodegas, inventarios y pedidos mediante una arquitectura robusta y segura (Clean Architecture + MediatR).

## Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)

## Configuración y Ejecución

### 1. Preparar la Base de Datos
El proyecto requiere una base de datos PostgreSQL. Debes actualizar el archivo `appsettings.Development.json` con las credenciales reales de tu servidor local.

Abre `src/PEIA.Web/appsettings.Development.json` y cambia `TU_CONTRASEÑA_AQUI` por la contraseña de tu usuario postgres:
```json
"DefaultConnection": "Host=localhost;Database=peiadb;Username=postgres;Password=TU_CONTRASEÑA_AQUI"
```

### 2. Levantar la aplicación
Ejecuta el siguiente comando en la raíz del proyecto para compilar e iniciar el backend:

```powershell
dotnet run --project src\PEIA.Web
```

### 3. Migraciones y Seed (Generación automática de datos)
Al ejecutarse por primera vez, la aplicación (**SeedData.cs**) se encargará automáticamente de:
1. Crear la base de datos `peiadb` y todas sus tablas mediante Migraciones de EF Core.
2. Crear los roles principales (`Administrador`, `OperadorInventario`, `Logistica`, etc).
3. Crear 2 bodegas por defecto (`Bodega Norte`, `Bodega Sur`).
4. Insertar los **5 usuarios de prueba** con sus roles asignados.

Alternativamente, puedes usar el script de SQL directo:
Si prefieres hacerlo manualmente, puedes crear una base de datos vacía en PostgreSQL y ejecutar el archivo `migrations_script.sql` (que también incluye los usuarios iniciales).

### 4. Acceder al Frontend
Abre tu navegador y entra a:
👉 **[https://localhost:5001](https://localhost:5001)**

### Usuarios de Prueba (Contraseña para todos: `Peia2025!`)
- **Admin**: `admin` (`admin@peia.com`)
- **Inventario**: `inventario` (`inventario@peia.com`)
- **Logística**: `logistica` (`logistica@peia.com`)
- **Reportes**: `reportes` (`reportes@peia.com`)
- **Supervisor**: `supervisor` (`supervisor@peia.com`)

---

## Equipo de desarrollo

| Clave | Nombre | Rol |
|---|---|---|
| AG | Ariel G. | Lider / Scrum Master / UX-UI / Modulo ERP |
| JDS | Julian S. | Backend — Inventario y Base de datos |
| JMVA | Jose M. | Backend — Logistica y SLAs |
| JMM | Jennifer Muñoz. | Frontend — Dashboard y Reportes |
| MJSG | Mariano S. | Backend — Automatizacion y DevOps |
| JCRF | Jose C. | Backend — Prediccion, Camaras y QA |

---

## Estado del proyecto

Consulta el archivo [PROGRESO.md](./PROGRESO.md) para ver el estado actual de cada tarea por fase y por integrante.

---

## Tecnologias utilizadas

- .NET 8 / C# 12
- Frontend nativo HTML/CSS/JS
- Entity Framework Core 8 + PostgreSQL
- ASP.NET Identity + JWT Bearer
- MediatR 12
- SignalR
- ML.NET 3
- xUnit (pruebas)
- Swagger / OpenAPI
