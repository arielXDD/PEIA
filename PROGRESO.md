ecoha esor, atrfeO

# PEIA — Seguimiento de Avance del Proyecto

> **INSTRUCCION PARA EL EQUIPO:**
> Este documento es la fuente de verdad del avance del proyecto.
> Cada integrante **debe actualizar su estado** cada vez que comience, avance o termine una tarea.
> No esperes a que el lider te lo pida — actualiza conforme trabajas.
>
> **Estados disponibles:**
>
> - `[ ]` — Pendiente (no iniciado)
> - `[/]` — En progreso
> - `[x]` — Completado
>
> **Como actualizar una tarea:**
> Cambia el estado entre corchetes y agrega la fecha en la columna correspondiente.
> Ejemplo: `[x]` Fecha: `18/06/20	26`

---

## Responsables

| Clave    | Nombre      | Area principal                                    |
| -------- | ----------- | ------------------------------------------------- |
| **AG**   | Ariel G.    | Lider / Scrum Master / UX-UI / Modulo ERP         |
| **JDS**  | Julian S.   | Backend — Inventario, Multi-centro, Base de datos |
| **JMVA** | Jose M.     | Backend — Logistica, Rastreo, SLAs                |
| **JMM**  | Jennifer M. | Frontend — Dashboard, Reportes, UI general        |
| **MJSG** | Mariano S.  | Backend — Automatizacion, Notificaciones, DevOps  |
| **JCRF** | Jose C.     | Backend — Prediccion, Camaras, Seguridad, QA      |

---

## Fase 0 — Infraestructura base (Prioridad ALTA)

> Debe completarse antes de que cualquier modulo empiece a codificarse.
> Responsables principales: **AG + JDS**

| Estado | Tarea                                                                 | Responsable | Ultima actualizacion |
| ------ | --------------------------------------------------------------------- | ----------- | -------------------- |
| `[x]`  | Crear solucion`.slnx` y estructura de proyectos                       | AG          | 18/06/2026           |
| `[x]`  | Configurar referencias entre proyectos                                | AG          | 18/06/2026           |
| `[x]`  | Instalar paquetes NuGet base (EF Core, MediatR, JWT, SignalR, ML.NET) | AG          | 18/06/2026           |
| `[x]`  | Crear`PeiaDbContext` con DbSets base                                  | JDS         | 03/07/2026           |
| `[x]`  | Definir entidades base:`Usuario`, `Centro`, `Rol`, `UsuarioCentro`    | JDS         | 03/07/2026           |
| `[x]`  | Configurar ASP.NET Identity sobre`PeiaDbContext`                      | AG          | 06/07/2026           |
| `[x]`  | Configurar JWT Bearer en`Program.cs`                                  | AG          | 06/07/2026           |
| `[x]`  | Configurar MediatR en`Program.cs`                                     | AG          | 06/07/2026           |
| `[x]`  | Configurar SignalR en`Program.cs`                                     | MJSG        | 06/07/2026           |
| `[x]`  | Configurar Swagger/OpenAPI en`Program.cs`                             | AG          | 06/07/2026           |
| `[x]`  | Crear primera migracion de base de datos                              | JDS         | 06/07/2026           |
| `[x]`  | Crear`SeedData` con usuario admin y centros iniciales                 | JDS         | 06/07/2026           |
| `[x]`  | Verificar que`dotnet run` levanta sin errores                         | AG          | 06/07/2026           |

---

## Fase 1 — ERP Core e Inventario (Sprint 1-2)

### Modulo ERP — `PEIA.Modules.ERP`

> Responsable: **AG**

| Estado | Tarea                                                        | Ultima actualizacion |
| ------ | ------------------------------------------------------------ | -------------------- |
| `[x]`  | CRUD de Usuarios (crear, editar, desactivar)                 | 06/07/2026           |
| `[x]`  | CRUD de Roles y permisos                                     | 06/07/2026           |
| `[x]`  | Asignacion de usuarios a centros (maximo 2)                  | 06/07/2026           |
| `[x]`  | Cambio de centro activo sin cerrar sesion                    | 06/07/2026           |
| `[x]`  | Endpoints REST:`/api/usuarios`, `/api/roles`, `/api/centros` | 06/07/2026           |
| `[ ]`  | Handlers MediatR para cada operacion                         | —                    |
| `[x]`  | Validaciones de negocio (maximo 2 centros)                   | 06/07/2026           |

### Modulo Inventario — `PEIA.Modules.Inventory`

> Responsable: **JDS**

| Estado | Tarea                                                                     | Ultima actualizacion |
| ------ | ------------------------------------------------------------------------- | -------------------- |
| `[x]`  | Entidades:`Producto`, `Categoria`, `Stock`, `Movimiento`                  | 03/07/2026           |
| `[x]`  | CRUD de productos por centro                                              | 06/07/2026           |
| `[x]`  | Control de stock por centro (entrada/salida/ajuste, sin negativos)        | 06/07/2026           |
| `[x]`  | Historial de movimientos                                                  | 06/07/2026           |
| `[ ]`  | Vista consolidada multi-centro (admin)                                    | —                    |
| `[x]`  | Endpoints REST:`/api/inventario/productos`, `/categorias`, `/movimientos` | 06/07/2026           |
| `[x]`  | Alertas de stock minimo (evento MediatR)                                  | 06/07/2026           |

### Frontend Fase 1 — `PEIA.Web`

> Responsable: **JMM**

| Estado | Tarea                                                    | Ultima actualizacion |
| ------ | -------------------------------------------------------- | -------------------- |
| `[x]`  | Pantalla de Login (diseno segun UX)                      | 01/07/2026           |
| `[x]`  | Dashboard principal (tarjetas de resumen)                | 01/07/2026           |
| `[x]`  | Pantalla de gestion de usuarios                          | 01/07/2026           |
| `[x]`  | Pantalla de gestion de roles                             | 01/07/2026           |
| `[x]`  | Pantalla de inventario y productos                       | 01/07/2026           |
| `[x]`  | Selector de centro activo en navbar                      | 01/07/2026           |
| `[x]`  | Componente de tabla reutilizable con paginacion          | 01/07/2026           |
| `[x]`  | Componente de formulario modal reutilizable              | 01/07/2026           |
| `[x]`  | Pantalla de Configuracion (5 tabs, conectada a API real) | 06/07/2026           |
| `[x]`  | Pantalla de Bitacora / Auditoria (carcasa)               | 03/07/2026           |

---

## Fase 2 — Logistica y Reportes (Sprint 3-4)

### Modulo Logistica — `PEIA.Modules.Logistics`

> Responsable: **JMVA**

| Estado | Tarea                                                    | Ultima actualizacion |
| ------ | -------------------------------------------------------- | -------------------- |
| `[x]`  | Entidades:`Pedido`, `Ruta`, `SLA`, `EntregaEstado`       | 01/07/2026           |
| `[x]`  | Registro y seguimiento de pedidos                        | 01/07/2026           |
| `[x]`  | Asignacion de rutas y transportistas                     | 01/07/2026           |
| `[x]`  | Control de SLAs (fechas limite y alertas)                | 01/07/2026           |
| `[x]`  | Rastreo de estado de entrega en tiempo real              | 01/07/2026           |
| `[x]`  | Endpoints REST:`/api/pedidos`, `/api/rutas`, `/api/slas` | 01/07/2026           |
| `[x]`  | Integracion con evento SignalR para actualizaciones      | 06/07/2026           |

### Modulo Reportes — `PEIA.Modules.Reports`

> Responsable: **JMM** (frontend) + **JMVA** (datos)

| Estado | Tarea                                        | Ultima actualizacion |
| ------ | -------------------------------------------- | -------------------- |
| `[x]`  | Reporte de inventario por centro             | 01/07/2026           |
| `[x]`  | Reporte de movimientos por rango de fecha    | 01/07/2026           |
| `[x]`  | Reporte de pedidos y estado logistico        | 01/07/2026           |
| `[x]`  | Exportacion a PDF / Excel                    | 01/07/2026           |
| `[x]`  | Graficas en dashboard (linea, barra, pastel) | 01/07/2026           |
| `[x]`  | Endpoint:`/api/reportes/*`                   | 01/07/2026           |

---

## Fase 3 — Automatizacion y Notificaciones (Sprint 5)

### Modulo Automatizacion — `PEIA.Modules.Automation`

> Responsable: **MJSG**

| Estado | Tarea                                                                               | Ultima actualizacion |
| ------ | ----------------------------------------------------------------------------------- | -------------------- |
| `[x]`  | Configurar hub de SignalR (`PeiaHub`)                                               | 06/07/2026           |
| `[x]`  | Notificaciones en tiempo real de stock critico                                      | 06/07/2026           |
| `[x]`  | Notificaciones de SLA vencido                                                       | 06/07/2026           |
| `[x]`  | Notificaciones de nuevos pedidos                                                    | 06/07/2026           |
| `[ ]`  | Reglas de automatizacion configurables por admin                                    | —                    |
| `[x]`  | Panel de notificaciones en UI, persistidas en BD y en vivo por SignalR (JMM + MJSG) | 06/07/2026           |
| `[x]`  | Pipeline CI/CD basico (GitHub Actions)                                              | 03/07/2026           |
| `[x]`  | Configuracion de entorno de pruebas                                                 | 06/07/2026           |

---

## Fase 4 — Prediccion y Camaras (Sprint 6)

### Modulo Prediccion — `PEIA.Modules.Prediction`

> Responsable: **JCRF**

| Estado | Tarea                                                                    | Ultima actualizacion |
| ------ | ------------------------------------------------------------------------ | -------------------- |
| `[x]`  | Modelo ML.NET de prediccion de demanda                                   | 06/07/2026           |
| `[x]`  | Entrenamiento con datos historicos de inventario (con fallback simulado) | 06/07/2026           |
| `[x]`  | Endpoint de prediccion:`/api/predicciones`                               | 06/07/2026           |
| `[x]`  | Visualizacion de predicciones en dashboard                               | 06/07/2026           |
| `[ ]`  | Integracion con modulo de camaras (si aplica)                            | —                    |
| `[x]`  | Pruebas de precision del modelo                                          | 06/07/2026           |

---

## QA y Pruebas — `PEIA.Tests`

> Responsable principal: **JCRF** (con apoyo de todos)

| Estado | Tarea                                                                                   | Responsable | Ultima actualizacion |
| ------ | --------------------------------------------------------------------------------------- | ----------- | -------------------- |
| `[ ]`  | Pruebas unitarias de handlers MediatR (ERP)                                             | AG          | —                    |
| `[x]`  | Pruebas unitarias de logica de inventario                                               | JDS         | 06/07/2026           |
| `[x]`  | Pruebas unitarias de logistica y SLAs                                                   | JMVA        | 01/07/2026           |
| `[x]`  | Pruebas de integracion de endpoints REST (WebApplicationFactory: Predicciones, Camaras) | JCRF        | 06/07/2026           |
| `[ ]`  | Pruebas de carga basicas                                                                | JCRF        | —                    |

---

## Registro de actualizaciones

> Cada vez que hagas un cambio significativo al proyecto, anotalo aqui con fecha, tu clave y que hiciste.

| Fecha      | Clave  | Descripcion del avance                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ---------- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 18/06/2026 | AG     | Estructura inicial de la solucion creada. 10 proyectos compilando. NuGet instalados. Subida a GitHub.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| 01/07/2026 | JMM    | Frontend completo: Login, Dashboard, Usuarios, Roles, Inventario, Reportes (con export PDF/Excel), Notificaciones. Componentes reutilizables (DataTable con paginacion, Modal de formulario). Navegacion sidebar funcional.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| 03/07/2026 | Equipo | Backend: Agregados módulos de logística (Controladores, Modelos, Pruebas), controladores de reportes y entidades de inventario. Actualización de PeiaDbContext y Entidades Base.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| 03/07/2026 | AG     | Frontend: Carcasa de Configuración (5 tabs: General, Notificaciones, Seguridad, Integraciones, Sistema) y Bitacora (tabla de auditoria con filtros, KPIs). Links del sidebar actualizados en todas las paginas.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| 06/07/2026 | AG     | **Backend:** ASP.NET Identity (Usuario/Rol/Centro/UsuarioCentro) + JWT Bearer + Swagger con soporte Bearer + MediatR + SignalR (PeiaHub) registrados en Program.cs; seed inicial de roles/centros/usuarios; migraciones automaticas al levantar el servidor (excepto en Testing). DbContext completo para Identity, logistica, inventario y configuracion; migraciones `AddInventoryInfrastructure`, `AddSystemSettings` y `AddNotificaciones`. Tabla `SystemSettings` para configuracion persistente. **ERP:** CRUD de centros/roles/usuarios (`/api/centros`, `/api/roles`, `/api/usuarios`), asignacion de usuario a maximo 2 centros, cambio de centro activo (`PUT /api/usuarios/centro-activo`). **Inventario:** CRUD de categorias y productos, stock por producto y centro, entradas/salidas/ajustes con validacion anti-negativo, historial de movimientos, alertas de stock minimo via MediatR. **Notificaciones:** entidad `Notificacion` + migracion, endpoint `/api/notificaciones` (listar, marcar leida, marcar todas, eliminar); los 3 handlers de SignalR (stock critico, pedido nuevo, SLA vencido) ahora persisten cada alerta en BD ademas de emitirla en vivo; worker `SlaMonitorService` marcando SLAs incumplidos. **Configuracion:** endpoints `/api/configuracion/empresa`, `/preferencias`, `/notificaciones`, `/seguridad` (GET+PUT) con persistencia real en `SystemSettings` como JSON. **Sistema:** `/api/sistema/salud` con estado de BD, migraciones aplicadas/pendientes y SignalR. **Prediccion/ML:** `PredictionService` usa movimientos reales de inventario cuando hay datos suficientes, con fallback simulado si no; prediccion global y por producto. **Reportes:** `GetInventarioReport` y `GetMovimientosReport` migrados de datos mock a consultas reales sobre Productos/Stocks/Categorias/Movimientos. **Frontend conectado (5 pantallas nuevas + cliente en vivo):** `notificaciones.js` conectado a la API real con cliente SignalR en vivo (nuevo helper `PEIA.connectHub()` en `api.js`); `configuracion.js` conectado a empresa/centros (CRUD)/preferencias/notificaciones/seguridad/salud del sistema; `reportes.js` conectado a inventario/movimientos/pedidos reales con export PDF/Excel; `prediccion.js` conectado a historico/pronostico ML.NET/resumen/tabla de productos. `usuarios.js`, `roles.js`, `inventario.js` y `pedidos.js` ya conectados de sesiones previas. **DevOps:** pipeline `.github/workflows/dotnet-ci.yml` (restore, build Release, test Release). **QA:** 26/26 tests pasando (`LogisticsTests`, `ErpTests`, `InventoryTests`, `ConfiguracionTests`, `PredictionServiceTests`, `PrediccionControllerTests`, `CamerasControllerTests`) incluyendo pruebas de integracion con `CustomWebApplicationFactory`. **Verificacion de hoy:** `node --check` en `api.js`, `notificaciones.js`, `configuracion.js`, `reportes.js`, `prediccion.js`; `dotnet build PEIA.slnx` correcto (0 errores); `dotnet test PEIA.slnx` correcto (26/26); verificacion manual end-to-end contra servidor real con PostgreSQL (login, alta de producto y movimiento de stock disparando notificacion persistida y visible via API, roundtrip GET/PUT de configuracion, reportes y predicciones devolviendo datos reales). |
|            |        |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
