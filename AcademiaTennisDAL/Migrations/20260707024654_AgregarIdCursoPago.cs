using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIdCursoPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCurso",
                table: "Pagos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_IdCurso",
                table: "Pagos",
                column: "IdCurso");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Cursos_IdCurso",
                table: "Pagos",
                column: "IdCurso",
                principalTable: "Cursos",
                principalColumn: "IdCurso");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Cursos_IdCurso",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_IdCurso",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "IdCurso",
                table: "Pagos");
        }
    }
}
