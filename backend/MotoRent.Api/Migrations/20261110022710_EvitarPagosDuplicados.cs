using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRent.Api.Migrations
{
    /// <inheritdoc />
    public partial class EvitarPagosDuplicados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pagos_AlquilerId",
                table: "Pagos");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_AlquilerId_DiaAlquilado",
                table: "Pagos",
                columns: new[] { "AlquilerId", "DiaAlquilado" },
                unique: true,
                filter: "[Estado] IN ('Pendiente', 'Confirmado')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pagos_AlquilerId_DiaAlquilado",
                table: "Pagos");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_AlquilerId",
                table: "Pagos",
                column: "AlquilerId");
        }
    }
}
