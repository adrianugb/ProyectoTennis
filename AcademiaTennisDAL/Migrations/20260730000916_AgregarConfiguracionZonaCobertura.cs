using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConfiguracionZonaCobertura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Especialidad",
                table: "Profesores");

            migrationBuilder.AlterColumn<decimal>(
                name: "CostoAdicional",
                table: "ZonasCobertura",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AddColumn<decimal>(
                name: "LatitudCentro",
                table: "ZonasCobertura",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LongitudCentro",
                table: "ZonasCobertura",
                type: "decimal(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RadioMaximoKm",
                table: "ZonasCobertura",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TarifaPorKm",
                table: "ZonasCobertura",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitud",
                table: "UbicacionesAlumno",
                type: "decimal(10,7)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitud",
                table: "UbicacionesAlumno",
                type: "decimal(10,7)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LatitudCentro",
                table: "ZonasCobertura");

            migrationBuilder.DropColumn(
                name: "LongitudCentro",
                table: "ZonasCobertura");

            migrationBuilder.DropColumn(
                name: "RadioMaximoKm",
                table: "ZonasCobertura");

            migrationBuilder.DropColumn(
                name: "TarifaPorKm",
                table: "ZonasCobertura");

            migrationBuilder.AlterColumn<decimal>(
                name: "CostoAdicional",
                table: "ZonasCobertura",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitud",
                table: "UbicacionesAlumno",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitud",
                table: "UbicacionesAlumno",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)");

            migrationBuilder.AddColumn<string>(
                name: "Especialidad",
                table: "Profesores",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
