using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarSolicitudesCurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NombreCurso",
                table: "SolicitudesCurso",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Nivel",
                table: "SolicitudesCurso",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "SolicitudesCurso",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Disponibilidad",
                table: "SolicitudesCurso",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Comentarios",
                table: "SolicitudesCurso",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CantidadLecciones",
                table: "SolicitudesCurso",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DireccionDomicilio",
                table: "SolicitudesCurso",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "EsADomicilio",
                table: "SolicitudesCurso",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPropuesta",
                table: "SolicitudesCurso",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRespuestaAlumno",
                table: "SolicitudesCurso",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraFinPropuesta",
                table: "SolicitudesCurso",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraInicioPropuesta",
                table: "SolicitudesCurso",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdCanchaPropuesta",
                table: "SolicitudesCurso",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdProfesorPropuesto",
                table: "SolicitudesCurso",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modalidad",
                table: "SolicitudesCurso",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ObservacionesAcademia",
                table: "SolicitudesCurso",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "RequiereEquipo",
                table: "SolicitudesCurso",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DisponibilidadesSolicitud",
                columns: table => new
                {
                    IdDisponibilidadSolicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdSolicitudCurso = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HoraDesde = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    HoraHasta = table.Column<TimeSpan>(type: "time(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisponibilidadesSolicitud", x => x.IdDisponibilidadSolicitud);
                    table.ForeignKey(
                        name: "FK_DisponibilidadesSolicitud_SolicitudesCurso_IdSolicitudCurso",
                        column: x => x.IdSolicitudCurso,
                        principalTable: "SolicitudesCurso",
                        principalColumn: "IdSolicitudCurso",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCurso_IdCanchaPropuesta",
                table: "SolicitudesCurso",
                column: "IdCanchaPropuesta");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCurso_IdProfesorPropuesto",
                table: "SolicitudesCurso",
                column: "IdProfesorPropuesto");

            migrationBuilder.CreateIndex(
                name: "IX_DisponibilidadesSolicitud_IdSolicitudCurso",
                table: "DisponibilidadesSolicitud",
                column: "IdSolicitudCurso");

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesCurso_Canchas_IdCanchaPropuesta",
                table: "SolicitudesCurso",
                column: "IdCanchaPropuesta",
                principalTable: "Canchas",
                principalColumn: "IdCancha");

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesCurso_Profesores_IdProfesorPropuesto",
                table: "SolicitudesCurso",
                column: "IdProfesorPropuesto",
                principalTable: "Profesores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesCurso_Canchas_IdCanchaPropuesta",
                table: "SolicitudesCurso");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesCurso_Profesores_IdProfesorPropuesto",
                table: "SolicitudesCurso");

            migrationBuilder.DropTable(
                name: "DisponibilidadesSolicitud");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesCurso_IdCanchaPropuesta",
                table: "SolicitudesCurso");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesCurso_IdProfesorPropuesto",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "CantidadLecciones",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "DireccionDomicilio",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "EsADomicilio",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "FechaPropuesta",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "FechaRespuestaAlumno",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "HoraFinPropuesta",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "HoraInicioPropuesta",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "IdCanchaPropuesta",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "IdProfesorPropuesto",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "Modalidad",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "ObservacionesAcademia",
                table: "SolicitudesCurso");

            migrationBuilder.DropColumn(
                name: "RequiereEquipo",
                table: "SolicitudesCurso");

            migrationBuilder.AlterColumn<string>(
                name: "NombreCurso",
                table: "SolicitudesCurso",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Nivel",
                table: "SolicitudesCurso",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "SolicitudesCurso",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Disponibilidad",
                table: "SolicitudesCurso",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Comentarios",
                table: "SolicitudesCurso",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
