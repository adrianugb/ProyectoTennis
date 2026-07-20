using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTarifaAPeticionCurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdTarifaClase",
                table: "SolicitudesCurso",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioSolicitado",
                table: "SolicitudesCurso",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCurso_IdTarifaClase",
                table: "SolicitudesCurso",
                column: "IdTarifaClase");

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesCurso_TarifasClase_IdTarifaClase",
                table: "SolicitudesCurso",
                column: "IdTarifaClase",
                principalTable: "TarifasClase",
                principalColumn: "IdTarifaClase");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesCurso_TarifasClase_IdTarifaClase",
                table: "SolicitudesCurso");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesCurso_IdTarifaClase",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "IdTarifaClase",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "PrecioSolicitado",
                table: "SolicitudesCurso");
        }
    }
}
