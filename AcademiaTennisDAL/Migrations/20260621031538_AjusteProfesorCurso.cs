using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class AjusteProfesorCurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Profesores_IdProfesor",
                table: "Cursos");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_IdProfesor",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "IdProfesor",
                table: "Cursos");

            migrationBuilder.AddColumn<string>(
                name: "IdProfesorUserId",
                table: "Cursos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_IdProfesorUserId",
                table: "Cursos",
                column: "IdProfesorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_AspNetUsers_IdProfesorUserId",
                table: "Cursos",
                column: "IdProfesorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_AspNetUsers_IdProfesorUserId",
                table: "Cursos");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_IdProfesorUserId",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "IdProfesorUserId",
                table: "Cursos");

            migrationBuilder.AddColumn<int>(
                name: "IdProfesor",
                table: "Cursos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_IdProfesor",
                table: "Cursos",
                column: "IdProfesor");

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Profesores_IdProfesor",
                table: "Cursos",
                column: "IdProfesor",
                principalTable: "Profesores",
                principalColumn: "Id");
        }
    }
}
