using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTiposClaseYRelacionTarifas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoClase",
                table: "TarifasClase");

            migrationBuilder.AddColumn<int>(
                name: "IdTipoClase",
                table: "TarifasClase",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TiposClase",
                columns: table => new
                {
                    IdTipoClase = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposClase", x => x.IdTipoClase);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TarifasClase_IdTipoClase",
                table: "TarifasClase",
                column: "IdTipoClase");

            migrationBuilder.AddForeignKey(
                name: "FK_TarifasClase_TiposClase_IdTipoClase",
                table: "TarifasClase",
                column: "IdTipoClase",
                principalTable: "TiposClase",
                principalColumn: "IdTipoClase",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TarifasClase_TiposClase_IdTipoClase",
                table: "TarifasClase");

            migrationBuilder.DropTable(
                name: "TiposClase");

            migrationBuilder.DropIndex(
                name: "IX_TarifasClase_IdTipoClase",
                table: "TarifasClase");

            migrationBuilder.DropColumn(
                name: "IdTipoClase",
                table: "TarifasClase");

            migrationBuilder.AddColumn<string>(
                name: "TipoClase",
                table: "TarifasClase",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
