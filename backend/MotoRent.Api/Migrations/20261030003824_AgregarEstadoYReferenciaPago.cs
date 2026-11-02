using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRent.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoYReferenciaPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Pagos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferenciaExterna",
                table: "Pagos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "ReferenciaExterna",
                table: "Pagos");
        }
    }
}
