using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PEIA.Shared.Infra.Data;

#nullable disable

namespace PEIA.Shared.Infra.Migrations;

[DbContextAttribute(typeof(PeiaDbContext))]
[Migration("20260721210000_AddUserSessions")]
public partial class AddUserSessions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserSessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                JwtId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                FechaExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IpAddress = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                Revocada = table.Column<bool>(type: "boolean", nullable: false),
                FechaRevocacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserSessions", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserSessions_Usuarios_UsuarioId",
                    column: x => x.UsuarioId,
                    principalTable: "Usuarios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserSessions_JwtId",
            table: "UserSessions",
            column: "JwtId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserSessions_UsuarioId_Revocada_FechaExpiracion",
            table: "UserSessions",
            columns: new[] { "UsuarioId", "Revocada", "FechaExpiracion" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserSessions");
    }
}
