using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRent.Api.Migrations
{
    /// <inheritdoc />
    public partial class CrearSolicitudesAlquiler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudesAlquiler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    MotoId = table.Column<int>(type: "int", nullable: false),
                    FechaInicioSolicitada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpcionCompra = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesAlquiler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesAlquiler_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesAlquiler_Motos_MotoId",
                        column: x => x.MotoId,
                        principalTable: "Motos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAlquiler_ClienteId",
                table: "SolicitudesAlquiler",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAlquiler_MotoId",
                table: "SolicitudesAlquiler",
                column: "MotoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesAlquiler");
        }
    }
}
