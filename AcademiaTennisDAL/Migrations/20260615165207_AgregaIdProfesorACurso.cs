using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregaIdProfesorACurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_AspNetUsers_IdProfesor",
                table: "Cursos");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_IdProfesor",
                table: "Cursos");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraInicio",
                table: "Reservas",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AlterColumn<string>(
                name: "IdProfesor",
                table: "Cursos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Profesores_IdProfesor",
                table: "Cursos");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_IdProfesor",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "HoraInicio",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "IdProfesor",
                table: "Cursos");

            migrationBuilder.AlterColumn<string>(
                name: "IdProfesor",
                table: "Cursos",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_IdProfesor",
                table: "Cursos",
                column: "IdProfesor");

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_AspNetUsers_IdProfesor",
                table: "Cursos",
                column: "IdProfesor",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
