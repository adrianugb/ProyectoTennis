using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSolicitudCursoCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesCurso_Cursos_IdCurso",
                table: "SolicitudesCurso");

            migrationBuilder.AlterColumn<int>(
                name: "IdCurso",
                table: "SolicitudesCurso",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "NombreCurso",
                table: "SolicitudesCurso",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesCurso_Cursos_IdCurso",
                table: "SolicitudesCurso",
                column: "IdCurso",
                principalTable: "Cursos",
                principalColumn: "IdCurso");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesCurso_Cursos_IdCurso",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "NombreCurso",
                table: "SolicitudesCurso");

            migrationBuilder.AlterColumn<int>(
                name: "IdCurso",
                table: "SolicitudesCurso",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesCurso_Cursos_IdCurso",
                table: "SolicitudesCurso",
                column: "IdCurso",
                principalTable: "Cursos",
                principalColumn: "IdCurso",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
