using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PEIA.Shared.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddCapturasTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Capturas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CameraId = table.Column<int>(type: "integer", nullable: false),
                    NombreCamara = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Zona = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImagenUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    FechaCaptura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capturas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Capturas_FechaCaptura",
                table: "Capturas",
                column: "FechaCaptura");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Capturas");
        }
    }
}
