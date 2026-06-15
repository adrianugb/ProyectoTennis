using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarClaseProgramada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Cursos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "IdProfesor",
                table: "Cursos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClasesProgramadas",
                columns: table => new
                {
                    IdClaseProgramada = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCurso = table.Column<int>(type: "int", nullable: false),
                    FechaClase = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasesProgramadas", x => x.IdClaseProgramada);
                    table.ForeignKey(
                        name: "FK_ClasesProgramadas_Cursos_IdCurso",
                        column: x => x.IdCurso,
                        principalTable: "Cursos",
                        principalColumn: "IdCurso",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_IdProfesor",
                table: "Cursos",
                column: "IdProfesor");

            migrationBuilder.CreateIndex(
                name: "IX_ClasesProgramadas_IdCurso",
                table: "ClasesProgramadas",
                column: "IdCurso");

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_AspNetUsers_IdProfesor",
                table: "Cursos",
                column: "IdProfesor",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_AspNetUsers_IdProfesor",
                table: "Cursos");

            migrationBuilder.DropTable(
                name: "ClasesProgramadas");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_IdProfesor",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "IdProfesor",
                table: "Cursos");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Cursos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
