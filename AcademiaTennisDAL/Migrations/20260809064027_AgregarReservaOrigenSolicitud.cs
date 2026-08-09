using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarReservaOrigenSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdReservaOrigen",
                table: "SolicitudesCurso",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCurso_IdReservaOrigen",
                table: "SolicitudesCurso",
                column: "IdReservaOrigen");

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesCurso_Reservas_IdReservaOrigen",
                table: "SolicitudesCurso",
                column: "IdReservaOrigen",
                principalTable: "Reservas",
                principalColumn: "IdReserva");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesCurso_Reservas_IdReservaOrigen",
                table: "SolicitudesCurso");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesCurso_IdReservaOrigen",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "IdReservaOrigen",
                table: "SolicitudesCurso");
        }
    }
}
