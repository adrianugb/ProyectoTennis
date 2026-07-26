using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarProgresoAlumno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AreasMejora",
                table: "ProgresosAlumno",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "IdCurso",
                table: "ProgresosAlumno",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosAlumno_IdCurso",
                table: "ProgresosAlumno",
                column: "IdCurso");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgresosAlumno_Cursos_IdCurso",
                table: "ProgresosAlumno",
                column: "IdCurso",
                principalTable: "Cursos",
                principalColumn: "IdCurso");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgresosAlumno_Cursos_IdCurso",
                table: "ProgresosAlumno");

            migrationBuilder.DropIndex(
                name: "IX_ProgresosAlumno_IdCurso",
                table: "ProgresosAlumno");

            migrationBuilder.DropColumn(
                name: "AreasMejora",
                table: "ProgresosAlumno");

            migrationBuilder.DropColumn(
                name: "IdCurso",
                table: "ProgresosAlumno");
        }
    }
}
