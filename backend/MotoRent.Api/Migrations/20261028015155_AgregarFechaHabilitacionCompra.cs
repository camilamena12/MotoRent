using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MotoRent.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechaHabilitacionCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaHabilitacionCompra",
                table: "Alquileres",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaHabilitacionCompra",
                table: "Alquileres");
        }
    }
}
