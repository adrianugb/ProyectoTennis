using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarClasesDomicilioAMatricula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Monto",
                table: "Pagos",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AddColumn<decimal>(
                name: "CostoDesplazamiento",
                table: "Pagos",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoBase",
                table: "Pagos",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoDesplazamiento",
                table: "Matriculas",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DistanciaKm",
                table: "Matriculas",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "EsADomicilio",
                table: "Matriculas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IdUbicacionAlumno",
                table: "Matriculas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoTotal",
                table: "Matriculas",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCurso",
                table: "Matriculas",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_IdUbicacionAlumno",
                table: "Matriculas",
                column: "IdUbicacionAlumno");

            migrationBuilder.AddForeignKey(
                name: "FK_Matriculas_UbicacionesAlumno_IdUbicacionAlumno",
                table: "Matriculas",
                column: "IdUbicacionAlumno",
                principalTable: "UbicacionesAlumno",
                principalColumn: "IdUbicacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matriculas_UbicacionesAlumno_IdUbicacionAlumno",
                table: "Matriculas");

            migrationBuilder.DropIndex(
                name: "IX_Matriculas_IdUbicacionAlumno",
                table: "Matriculas");

            migrationBuilder.DropColumn(
                name: "CostoDesplazamiento",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "MontoBase",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "CostoDesplazamiento",
                table: "Matriculas");

            migrationBuilder.DropColumn(
                name: "DistanciaKm",
                table: "Matriculas");

            migrationBuilder.DropColumn(
                name: "EsADomicilio",
                table: "Matriculas");

            migrationBuilder.DropColumn(
                name: "IdUbicacionAlumno",
                table: "Matriculas");

            migrationBuilder.DropColumn(
                name: "MontoTotal",
                table: "Matriculas");

            migrationBuilder.DropColumn(
                name: "PrecioCurso",
                table: "Matriculas");

            migrationBuilder.AlterColumn<decimal>(
                name: "Monto",
                table: "Pagos",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");
        }
    }
}
