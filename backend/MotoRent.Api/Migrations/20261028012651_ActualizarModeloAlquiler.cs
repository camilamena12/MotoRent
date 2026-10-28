using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRent.Api.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarModeloAlquiler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrecioTotal",
                table: "Alquileres",
                newName: "PrecioPorDia");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaDevolucion",
                table: "Alquileres",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<bool>(
                name: "OpcionCompra",
                table: "Alquileres",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpcionCompra",
                table: "Alquileres");

            migrationBuilder.RenameColumn(
                name: "PrecioPorDia",
                table: "Alquileres",
                newName: "PrecioTotal");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaDevolucion",
                table: "Alquileres",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
