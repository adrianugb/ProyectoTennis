using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class RelacionPagoMatriculaReserva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimiento",
                table: "Pagos",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdMatricula",
                table: "Pagos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdReserva",
                table: "Pagos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_IdMatricula",
                table: "Pagos",
                column: "IdMatricula");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_IdReserva",
                table: "Pagos",
                column: "IdReserva");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Matriculas_IdMatricula",
                table: "Pagos",
                column: "IdMatricula",
                principalTable: "Matriculas",
                principalColumn: "IdMatricula");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Reservas_IdReserva",
                table: "Pagos",
                column: "IdReserva",
                principalTable: "Reservas",
                principalColumn: "IdReserva");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Matriculas_IdMatricula",
                table: "Pagos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Reservas_IdReserva",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_IdMatricula",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_IdReserva",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "IdMatricula",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "IdReserva",
                table: "Pagos");
        }
    }
}
