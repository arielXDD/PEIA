using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEIA.Shared.Infra.Migrations
{
    /// <inheritdoc />
    public partial class LotesYCaducidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCaducidad",
                table: "Stocks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lote",
                table: "Stocks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCaducidad",
                table: "Movimientos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lote",
                table: "Movimientos",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReglasAutomatizacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EventoOrigen = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Condicion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Accion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Responsable = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasAutomatizacion", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReglasAutomatizacion");

            migrationBuilder.DropColumn(
                name: "FechaCaducidad",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "Lote",
                table: "Stocks");

            migrationBuilder.DropColumn(
                name: "FechaCaducidad",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "Lote",
                table: "Movimientos");
        }
    }
}
