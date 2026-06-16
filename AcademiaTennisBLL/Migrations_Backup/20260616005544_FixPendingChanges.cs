using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Profesores_IdProfesor",
                table: "Cursos");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Profesores_TempId",
                table: "Profesores");

            migrationBuilder.DropColumn(
                name: "TempId",
                table: "Profesores");

            migrationBuilder.AlterColumn<int>(
                name: "IdProfesor",
                table: "Cursos",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Profesores_IdProfesor",
                table: "Cursos",
                column: "IdProfesor",
                principalTable: "Profesores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Profesores_IdProfesor",
                table: "Cursos");

            migrationBuilder.AddColumn<string>(
                name: "TempId",
                table: "Profesores",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "IdProfesor",
                table: "Cursos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Profesores_TempId",
                table: "Profesores",
                column: "TempId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Profesores_IdProfesor",
                table: "Cursos",
                column: "IdProfesor",
                principalTable: "Profesores",
                principalColumn: "TempId");
        }
    }
}
