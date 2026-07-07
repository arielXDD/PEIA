using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEIA.Shared.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CentroId = table.Column<Guid>(type: "uuid", nullable: false),
                    Leida = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_CentroId_FechaCreacion",
                table: "Notificaciones",
                columns: new[] { "CentroId", "FechaCreacion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificaciones");
        }
    }
}
