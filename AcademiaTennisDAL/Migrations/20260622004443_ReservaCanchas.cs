using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class ReservaCanchas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_AspNetUsers_IdProfesorUserId",
                table: "Cursos");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_AspNetUsers_IdAlumno",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_AspNetUsers_IdProfesor",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_IdProfesorUserId",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "IdProfesorUserId",
                table: "Cursos");

            migrationBuilder.AlterColumn<string>(
                name: "IdProfesor",
                table: "Reservas",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdAlumno",
                table: "Reservas",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_AspNetUsers_IdAlumno",
                table: "Reservas",
                column: "IdAlumno",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_AspNetUsers_IdProfesor",
                table: "Reservas",
                column: "IdProfesor",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Profesores_IdProfesor",
                table: "Cursos");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_AspNetUsers_IdAlumno",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_AspNetUsers_IdProfesor",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_IdProfesor",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "IdProfesor",
                table: "Cursos");

            migrationBuilder.AlterColumn<string>(
                name: "IdProfesor",
                table: "Reservas",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "IdAlumno",
                table: "Reservas",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_AspNetUsers_IdAlumno",
                table: "Reservas",
                column: "IdAlumno",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_AspNetUsers_IdProfesor",
                table: "Reservas",
                column: "IdProfesor",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
