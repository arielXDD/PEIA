using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEIA.Shared.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rutas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Origen = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Destino = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    DistanciaKm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rutas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UnidadMedida = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    StockMinimo = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Cliente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DireccionEntrega = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FechaPedido = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaEstimadaEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CentroId = table.Column<Guid>(type: "uuid", nullable: false),
                    RutaId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransportistaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedidos_Centros_CentroId",
                        column: x => x.CentroId,
                        principalTable: "Centros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedidos_Rutas_RutaId",
                        column: x => x.RutaId,
                        principalTable: "Rutas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Pedidos_Usuarios_TransportistaId",
                        column: x => x.TransportistaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Movimientos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    StockAnterior = table.Column<int>(type: "integer", nullable: false),
                    StockNuevo = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Referencia = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    FechaMovimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CentroId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movimientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movimientos_Centros_CentroId",
                        column: x => x.CentroId,
                        principalTable: "Centros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movimientos_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movimientos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    Ubicacion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CentroId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stocks_Centros_CentroId",
                        column: x => x.CentroId,
                        principalTable: "Centros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stocks_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntregaEstados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitud = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitud = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualizadoPorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntregaEstados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntregaEstados_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntregaEstados_Usuarios_ActualizadoPorId",
                        column: x => x.ActualizadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SLAs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TiempoLimite = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstadoSLA = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SLAs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SLAs_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nombre",
                table: "Categorias",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntregaEstados_ActualizadoPorId",
                table: "EntregaEstados",
                column: "ActualizadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_EntregaEstados_PedidoId",
                table: "EntregaEstados",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_CentroId_FechaMovimiento",
                table: "Movimientos",
                columns: new[] { "CentroId", "FechaMovimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_ProductoId",
                table: "Movimientos",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_UsuarioId",
                table: "Movimientos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_CentroId",
                table: "Pedidos",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_Codigo",
                table: "Pedidos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_RutaId",
                table: "Pedidos",
                column: "RutaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_TransportistaId",
                table: "Pedidos",
                column: "TransportistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Sku",
                table: "Productos",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SLAs_PedidoId",
                table: "SLAs",
                column: "PedidoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_CentroId",
                table: "Stocks",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_Stocks_ProductoId_CentroId",
                table: "Stocks",
                columns: new[] { "ProductoId", "CentroId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntregaEstados");

            migrationBuilder.DropTable(
                name: "Movimientos");

            migrationBuilder.DropTable(
                name: "SLAs");

            migrationBuilder.DropTable(
                name: "Stocks");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Rutas");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
