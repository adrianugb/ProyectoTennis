using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDomicilioAPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DistanciaKm",
                table: "Pagos",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "EsADomicilio",
                table: "Pagos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IdUbicacionAlumno",
                table: "Pagos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_IdUbicacionAlumno",
                table: "Pagos",
                column: "IdUbicacionAlumno");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_UbicacionesAlumno_IdUbicacionAlumno",
                table: "Pagos",
                column: "IdUbicacionAlumno",
                principalTable: "UbicacionesAlumno",
                principalColumn: "IdUbicacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_UbicacionesAlumno_IdUbicacionAlumno",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_IdUbicacionAlumno",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "DistanciaKm",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "EsADomicilio",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "IdUbicacionAlumno",
                table: "Pagos");
        }
    }
}
