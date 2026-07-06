-- Script de base de datos para el módulo de Logística, Rastreo y SLAs (PostgreSQL)

-- 1. Tabla de Rutas
CREATE TABLE IF NOT EXISTS "Rutas" (
    "Id" UUID NOT NULL,
    "Nombre" VARCHAR(150) NOT NULL,
    "Origen" VARCHAR(250) NOT NULL,
    "Destino" VARCHAR(250) NOT NULL,
    "DistanciaKm" DECIMAL(10, 2) NOT NULL,
    "Activa" BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaCreacion" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "PK_Rutas" PRIMARY KEY ("Id")
);

-- 2. Tabla de Pedidos
CREATE TABLE IF NOT EXISTS "Pedidos" (
    "Id" UUID NOT NULL,
    "Codigo" VARCHAR(50) NOT NULL,
    "Cliente" VARCHAR(200) NOT NULL,
    "DireccionEntrega" VARCHAR(300) NOT NULL,
    "FechaPedido" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "FechaEstimadaEntrega" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Estado" VARCHAR(50) NOT NULL, -- Ej: 'Creado', 'Asignado', 'EnRuta', 'Entregado', 'Cancelado'
    "CentroId" UUID NOT NULL,       -- Llave foránea a Centros
    "RutaId" UUID NULL,            -- Llave foránea a Rutas (opcional)
    "TransportistaId" UUID NULL,   -- Llave foránea a Usuarios (del rol de Logística/Repartidor)
    CONSTRAINT "PK_Pedidos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Pedidos_Centros_CentroId" FOREIGN KEY ("CentroId") REFERENCES "Centros" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Pedidos_Rutas_RutaId" FOREIGN KEY ("RutaId") REFERENCES "Rutas" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Pedidos_Usuarios_TransportistaId" FOREIGN KEY ("TransportistaId") REFERENCES "Usuarios" ("Id") ON DELETE SET NULL
);

-- Índices para búsquedas rápidas en Pedidos
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Pedidos_Codigo" ON "Pedidos" ("Codigo");
CREATE INDEX IF NOT EXISTS "IX_Pedidos_CentroId" ON "Pedidos" ("CentroId");
CREATE INDEX IF NOT EXISTS "IX_Pedidos_TransportistaId" ON "Pedidos" ("TransportistaId");

-- 3. Tabla de SLAs (Service Level Agreements)
CREATE TABLE IF NOT EXISTS "SLAs" (
    "Id" UUID NOT NULL,
    "PedidoId" UUID NOT NULL,
    "TiempoLimite" TIMESTAMP WITH TIME ZONE NOT NULL,
    "EstadoSLA" VARCHAR(50) NOT NULL, -- Ej: 'Cumplido', 'EnRiesgo', 'Incumplido'
    "FechaResolucion" TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT "PK_SLAs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SLAs_Pedidos_PedidoId" FOREIGN KEY ("PedidoId") REFERENCES "Pedidos" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_SLAs_PedidoId" ON "SLAs" ("PedidoId");

-- 4. Tabla de EntregaEstados (Historial de rastreo)
CREATE TABLE IF NOT EXISTS "EntregaEstados" (
    "Id" UUID NOT NULL,
    "PedidoId" UUID NOT NULL,
    "Estado" VARCHAR(50) NOT NULL,
    "Descripcion" VARCHAR(500) NULL,
    "Latitud" DECIMAL(9, 6) NULL,
    "Longitud" DECIMAL(9, 6) NULL,
    "FechaActualizacion" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ActualizadoPorId" UUID NULL, -- Usuario que actualiza el estado (ej: transportista)
    CONSTRAINT "PK_EntregaEstados" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EntregaEstados_Pedidos_PedidoId" FOREIGN KEY ("PedidoId") REFERENCES "Pedidos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_EntregaEstados_Usuarios_ActualizadoPorId" FOREIGN KEY ("ActualizadoPorId") REFERENCES "Usuarios" ("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_EntregaEstados_PedidoId" ON "EntregaEstados" ("PedidoId");
