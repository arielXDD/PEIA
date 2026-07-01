# PEIA — Seguimiento de Avance del Proyecto

> **INSTRUCCION PARA EL EQUIPO:**
> Este documento es la fuente de verdad del avance del proyecto.
> Cada integrante **debe actualizar su estado** cada vez que comience, avance o termine una tarea.
> No esperes a que el lider te lo pida — actualiza conforme trabajas.
>
> **Estados disponibles:**
> - `[ ]` — Pendiente (no iniciado)
> - `[/]` — En progreso
> - `[x]` — Completado
>
> **Como actualizar una tarea:**
> Cambia el estado entre corchetes y agrega la fecha en la columna correspondiente.
> Ejemplo: `[x]` Fecha: `18/06/2026`

---

## Responsables

| Clave | Nombre | Area principal |
|---|---|---|
| **AG** | Ariel G. | Lider / Scrum Master / UX-UI / Modulo ERP |
| **JDS** | Julian S. | Backend — Inventario, Multi-centro, Base de datos |
| **JMVA** | Jose M. | Backend — Logistica, Rastreo, SLAs |
| **JMM** | Jennifer M. | Frontend — Dashboard, Reportes, UI general |
| **MJSG** | Mariano S. | Backend — Automatizacion, Notificaciones, DevOps |
| **JCRF** | Jose C. | Backend — Prediccion, Camaras, Seguridad, QA |

---

## Fase 0 — Infraestructura base (Prioridad ALTA)

> Debe completarse antes de que cualquier modulo empiece a codificarse.
> Responsables principales: **AG + JDS**

| Estado | Tarea | Responsable | Ultima actualizacion |
|---|---|---|---|
| `[x]` | Crear solucion `.slnx` y estructura de proyectos | AG | 18/06/2026 |
| `[x]` | Configurar referencias entre proyectos | AG | 18/06/2026 |
| `[x]` | Instalar paquetes NuGet base (EF Core, MediatR, JWT, SignalR, ML.NET) | AG | 18/06/2026 |
| `[ ]` | Crear `PeiaDbContext` con DbSets base | JDS | — |
| `[ ]` | Definir entidades base: `Usuario`, `Centro`, `Rol`, `UsuarioCentro` | JDS | — |
| `[ ]` | Configurar ASP.NET Identity sobre `PeiaDbContext` | AG | — |
| `[ ]` | Configurar JWT Bearer en `Program.cs` | AG | — |
| `[ ]` | Configurar MediatR en `Program.cs` | AG | — |
| `[ ]` | Configurar SignalR en `Program.cs` | MJSG | — |
| `[ ]` | Configurar Swagger/OpenAPI en `Program.cs` | AG | — |
| `[ ]` | Crear primera migracion de base de datos | JDS | — |
| `[ ]` | Crear `SeedData` con usuario admin y centros iniciales | JDS | — |
| `[ ]` | Verificar que `dotnet run` levanta sin errores | AG | — |

---

## Fase 1 — ERP Core e Inventario (Sprint 1-2)

### Modulo ERP — `PEIA.Modules.ERP`
> Responsable: **AG**

| Estado | Tarea | Ultima actualizacion |
|---|---|---|
| `[ ]` | CRUD de Usuarios (crear, editar, desactivar) | — |
| `[ ]` | CRUD de Roles y permisos | — |
| `[ ]` | Asignacion de usuarios a centros | — |
| `[ ]` | Cambio de centro activo sin cerrar sesion | — |
| `[ ]` | Endpoints REST: `/api/usuarios`, `/api/roles`, `/api/centros` | — |
| `[ ]` | Handlers MediatR para cada operacion | — |
| `[ ]` | Validaciones de negocio (maximo 2 centros) | — |

### Modulo Inventario — `PEIA.Modules.Inventory`
> Responsable: **JDS**

| Estado | Tarea | Ultima actualizacion |
|---|---|---|
| `[ ]` | Entidades: `Producto`, `Categoria`, `Stock`, `Movimiento` | — |
| `[ ]` | CRUD de productos por centro | — |
| `[ ]` | Control de stock por centro (entrada/salida) | — |
| `[ ]` | Historial de movimientos | — |
| `[ ]` | Vista consolidada multi-centro (admin) | — |
| `[ ]` | Endpoints REST: `/api/productos`, `/api/stock`, `/api/movimientos` | — |
| `[ ]` | Alertas de stock minimo (evento MediatR) | — |

### Frontend Fase 1 — `PEIA.Web`
> Responsable: **JMM**

| Estado | Tarea | Ultima actualizacion |
|---|---|---|
| `[x]` | Pantalla de Login (diseno segun UX) | 01/07/2026 |
| `[x]` | Dashboard principal (tarjetas de resumen) | 01/07/2026 |
| `[x]` | Pantalla de gestion de usuarios | 01/07/2026 |
| `[x]` | Pantalla de gestion de roles | 01/07/2026 |
| `[x]` | Pantalla de inventario y productos | 01/07/2026 |
| `[x]` | Selector de centro activo en navbar | 01/07/2026 |
| `[x]` | Componente de tabla reutilizable con paginacion | 01/07/2026 |
| `[x]` | Componente de formulario modal reutilizable | 01/07/2026 |

---

## Fase 2 — Logistica y Reportes (Sprint 3-4)

### Modulo Logistica — `PEIA.Modules.Logistics`
> Responsable: **JMVA**

| Estado | Tarea | Ultima actualizacion |
|---|---|---|
| `[ ]` | Entidades: `Pedido`, `Ruta`, `SLA`, `EntregaEstado` | — |
| `[ ]` | Registro y seguimiento de pedidos | — |
| `[ ]` | Asignacion de rutas y transportistas | — |
| `[ ]` | Control de SLAs (fechas limite y alertas) | — |
| `[ ]` | Rastreo de estado de entrega en tiempo real | — |
| `[ ]` | Endpoints REST: `/api/pedidos`, `/api/rutas`, `/api/slas` | — |
| `[ ]` | Integracion con evento SignalR para actualizaciones | — |

### Modulo Reportes — `PEIA.Modules.Reports`
> Responsable: **JMM** (frontend) + **JMVA** (datos)

| Estado | Tarea | Ultima actualizacion |
|---|---|---|
| `[x]` | Reporte de inventario por centro | 01/07/2026 |
| `[x]` | Reporte de movimientos por rango de fecha | 01/07/2026 |
| `[x]` | Reporte de pedidos y estado logistico | 01/07/2026 |
| `[x]` | Exportacion a PDF / Excel | 01/07/2026 |
| `[x]` | Graficas en dashboard (linea, barra, pastel) | 01/07/2026 |
| `[ ]` | Endpoint: `/api/reportes/*` | — |

---

## Fase 3 — Automatizacion y Notificaciones (Sprint 5)

### Modulo Automatizacion — `PEIA.Modules.Automation`
> Responsable: **MJSG**

| Estado | Tarea | Ultima actualizacion |
|---|---|---|
| `[ ]` | Configurar hub de SignalR (`PeiaHub`) | — |
| `[ ]` | Notificaciones en tiempo real de stock critico | — |
| `[ ]` | Notificaciones de SLA vencido | — |
| `[ ]` | Notificaciones de nuevos pedidos | — |
| `[ ]` | Reglas de automatizacion configurables por admin | — |
| `[x]` | Panel de notificaciones en UI (JMM) | 01/07/2026 |
| `[ ]` | Pipeline CI/CD basico (GitHub Actions) | — |
| `[ ]` | Configuracion de entorno de pruebas | — |

---

## Fase 4 — Prediccion y Camaras (Sprint 6)

### Modulo Prediccion — `PEIA.Modules.Prediction`
> Responsable: **JCRF**

| Estado | Tarea | Ultima actualizacion |
|---|---|---|
| `[ ]` | Modelo ML.NET de prediccion de demanda | — |
| `[ ]` | Entrenamiento con datos historicos de inventario | — |
| `[ ]` | Endpoint de prediccion: `/api/predicciones` | — |
| `[ ]` | Visualizacion de predicciones en dashboard | — |
| `[ ]` | Integracion con modulo de camaras (si aplica) | — |
| `[ ]` | Pruebas de precision del modelo | — |

---

## QA y Pruebas — `PEIA.Tests`
> Responsable principal: **JCRF** (con apoyo de todos)

| Estado | Tarea | Responsable | Ultima actualizacion |
|---|---|---|---|
| `[ ]` | Pruebas unitarias de handlers MediatR (ERP) | AG | — |
| `[ ]` | Pruebas unitarias de logica de inventario | JDS | — |
| `[ ]` | Pruebas unitarias de logistica y SLAs | JMVA | — |
| `[ ]` | Pruebas de integracion de endpoints REST | JCRF | — |
| `[ ]` | Pruebas de carga basicas | JCRF | — |

---

## Registro de actualizaciones

> Cada vez que hagas un cambio significativo al proyecto, anotalo aqui con fecha, tu clave y que hiciste.

| Fecha | Clave | Descripcion del avance |
|---|---|---|
| 18/06/2026 | AG | Estructura inicial de la solucion creada. 10 proyectos compilando. NuGet instalados. Subida a GitHub. |
| 01/07/2026 | JMM | Frontend completo: Login, Dashboard, Usuarios, Roles, Inventario, Reportes (con export PDF/Excel), Notificaciones. Componentes reutilizables (DataTable con paginacion, Modal de formulario). Navegacion sidebar funcional. |
| | | |
| | | |
| | | |
