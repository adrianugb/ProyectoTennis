using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class ModuloPromociones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ofertas",
                columns: table => new
                {
                    IdOferta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    ProductosAplicables = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ofertas", x => x.IdOferta);
                });

            migrationBuilder.CreateTable(
                name: "Cupones",
                columns: table => new
                {
                    IdCupon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LimiteUsos = table.Column<int>(type: "int", nullable: false),
                    UsosActuales = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    IdOferta = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cupones", x => x.IdCupon);
                    table.ForeignKey(
                        name: "FK_Cupones_Ofertas_IdOferta",
                        column: x => x.IdOferta,
                        principalTable: "Ofertas",
                        principalColumn: "IdOferta");
                });

            migrationBuilder.CreateTable(
                name: "CuponUsos",
                columns: table => new
                {
                    IdUso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCupon = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaUso = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuponUsos", x => x.IdUso);
                    table.ForeignKey(
                        name: "FK_CuponUsos_AspNetUsers_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CuponUsos_Cupones_IdCupon",
                        column: x => x.IdCupon,
                        principalTable: "Cupones",
                        principalColumn: "IdCupon",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cupones_IdOferta",
                table: "Cupones",
                column: "IdOferta");

            migrationBuilder.CreateIndex(
                name: "IX_CuponUsos_IdCupon",
                table: "CuponUsos",
                column: "IdCupon");

            migrationBuilder.CreateIndex(
                name: "IX_CuponUsos_IdUsuario",
                table: "CuponUsos",
                column: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuponUsos");

            migrationBuilder.DropTable(
                name: "Cupones");

            migrationBuilder.DropTable(
                name: "Ofertas");
        }
    }
}
