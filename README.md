# PEIA - Plataforma Empresarial Inteligente para Almacenes

## Descripcion

PEIA es un sistema de gestion empresarial orientado a almacenes, desarrollado como plataforma web bajo una arquitectura de Monolito Modular. El sistema esta disenado para operar sobre dos centros de almacenamiento de forma simultanea, permitiendo el control centralizado del inventario, la logistica de salida, la generacion de reportes y la prediccion de demanda mediante inteligencia artificial.

El proyecto esta desarrollado con .NET 8 en C#, siguiendo la metodologia Scrum con entregas incrementales organizadas en fases.

---

## Funcionalidades principales

- Gestion de usuarios, roles y permisos con soporte multi-centro
- Control de inventario por centro con historial de movimientos y alertas de stock minimo
- Seguimiento logistico de pedidos con control de SLAs y rastreo en tiempo real
- Generacion de reportes exportables en PDF y Excel
- Notificaciones en tiempo real mediante WebSockets (SignalR)
- Prediccion de demanda utilizando modelos de aprendizaje automatico (ML.NET)
- Panel administrativo con vista consolidada de ambos centros

---

## Arquitectura

El sistema sigue un patron de **Monolito Modular**. Cada modulo de negocio esta aislado en su propio proyecto de clase, con comunicacion entre modulos a traves de MediatR. Esto permite un despliegue sencillo como una sola aplicacion, con la posibilidad de extraer modulos a servicios independientes en el futuro.

```
PEIA.slnx
src/
  PEIA.Web                    -> Host principal (ASP.NET Core + Razor Pages)
  Shared/
    PEIA.Shared.Kernel        -> Contratos, DTOs y eventos compartidos
    PEIA.Shared.Infra         -> Base de datos, identidad y repositorios
  Modules/
    PEIA.Modules.ERP          -> Usuarios, roles y centros
    PEIA.Modules.Inventory    -> Inventario y movimientos
    PEIA.Modules.Logistics    -> Pedidos, rutas y SLAs
    PEIA.Modules.Reports      -> Reportes y graficas
    PEIA.Modules.Automation   -> Notificaciones y automatizacion
    PEIA.Modules.Prediction   -> Prediccion con ML.NET
tests/
  PEIA.Tests                  -> Pruebas unitarias y de integracion (xUnit)
```

---

## Requisitos para ejecutar el proyecto

Antes de clonar y ejecutar el proyecto, asegurate de tener instaladas las siguientes herramientas:

| Herramienta | Version minima | Descripcion |
|---|---|---|
| .NET SDK | 8.0 LTS | Entorno de ejecucion y compilacion principal |
| SQL Server | 2019 o superior | Motor de base de datos relacional |
| Visual Studio 2022 | 17.8 o superior | IDE recomendado (Community Edition es suficiente) |
| Git | 2.40 o superior | Control de versiones |

> SQL Server Express es suficiente para desarrollo local. Puedes descargarlo en https://www.microsoft.com/es-mx/sql-server/sql-server-downloads

---

## Configuracion inicial

1. Clona el repositorio:
   ```bash
   git clone https://github.com/arielXDD/PEIA.git
   cd PEIA
   ```

2. Configura la cadena de conexion a tu instancia de SQL Server en `src/PEIA.Web/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=PeiaDb;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

3. Aplica las migraciones de base de datos:
   ```bash
   dotnet ef database update --project src/Shared/PEIA.Shared.Infra --startup-project src/PEIA.Web
   ```

4. Ejecuta el proyecto:
   ```bash
   dotnet run --project src/PEIA.Web
   ```

5. Accede a la aplicacion en `https://localhost:5001` o revisa el puerto asignado en la consola.

---

## Equipo de desarrollo

| Clave | Nombre | Rol |
|---|---|---|
| AG | Ariel G. | Lider / Scrum Master / UX-UI / Modulo ERP |
| JDS | Julian S. | Backend — Inventario y Base de datos |
| JMVA | Jose M. | Backend — Logistica y SLAs |
| JMM | Jennifer M. | Frontend — Dashboard y Reportes |
| MJSG | Mariano S. | Backend — Automatizacion y DevOps |
| JCRF | Jose C. | Backend — Prediccion, Camaras y QA |

---

## Estado del proyecto

Consulta el archivo [PROGRESO.md](./PROGRESO.md) para ver el estado actual de cada tarea por fase y por integrante.

---

## Tecnologias utilizadas

- .NET 8 / C# 12
- ASP.NET Core (Razor Pages)
- Entity Framework Core 8 + SQL Server
- ASP.NET Identity + JWT Bearer
- MediatR 12
- SignalR
- ML.NET 3
- xUnit (pruebas)
- Swagger / OpenAPI
