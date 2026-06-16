using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class ModulosTerminados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertasAbandono",
                columns: table => new
                {
                    IdAlerta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlumno = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaAlerta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaGestion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccionTomada = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasAbandono", x => x.IdAlerta);
                    table.ForeignKey(
                        name: "FK_AlertasAbandono_AspNetUsers_IdAlumno",
                        column: x => x.IdAlumno,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Campeonatos",
                columns: table => new
                {
                    IdCampeonato = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Reglas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxParticipantes = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campeonatos", x => x.IdCampeonato);
                });

            migrationBuilder.CreateTable(
                name: "ConsultasChatbot",
                columns: table => new
                {
                    IdConsulta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MensajeUsuario = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RespuestaBot = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaConsulta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Resuelto = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultasChatbot", x => x.IdConsulta);
                    table.ForeignKey(
                        name: "FK_ConsultasChatbot_AspNetUsers_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EncuestasSatisfaccion",
                columns: table => new
                {
                    IdEncuesta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlumno = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdReserva = table.Column<int>(type: "int", nullable: false),
                    CalificacionClase = table.Column<int>(type: "int", nullable: false),
                    CalificacionProfesor = table.Column<int>(type: "int", nullable: false),
                    CalificacionInstalaciones = table.Column<int>(type: "int", nullable: false),
                    Comentarios = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Recomendaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaEncuesta = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncuestasSatisfaccion", x => x.IdEncuesta);
                    table.ForeignKey(
                        name: "FK_EncuestasSatisfaccion_AspNetUsers_IdAlumno",
                        column: x => x.IdAlumno,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EncuestasSatisfaccion_Reservas_IdReserva",
                        column: x => x.IdReserva,
                        principalTable: "Reservas",
                        principalColumn: "IdReserva");
                });

            migrationBuilder.CreateTable(
                name: "LogrosAlumno",
                columns: table => new
                {
                    IdLogro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlumno = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TipoLogro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaObtencion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogrosAlumno", x => x.IdLogro);
                    table.ForeignKey(
                        name: "FK_LogrosAlumno_AspNetUsers_IdAlumno",
                        column: x => x.IdAlumno,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreguntasFrecuentes",
                columns: table => new
                {
                    IdPregunta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pregunta = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Respuesta = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    VecesConsultada = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreguntasFrecuentes", x => x.IdPregunta);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    IdProducto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioAlquiler = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.IdProducto);
                });

            migrationBuilder.CreateTable(
                name: "ProgresosAlumno",
                columns: table => new
                {
                    IdProgreso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlumno = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdProfesor = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NivelSaque = table.Column<int>(type: "int", nullable: false),
                    NivelReves = table.Column<int>(type: "int", nullable: false),
                    NivelDerecha = table.Column<int>(type: "int", nullable: false),
                    NivelVolea = table.Column<int>(type: "int", nullable: false),
                    NivelMovimiento = table.Column<int>(type: "int", nullable: false),
                    NivelTactica = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NivelGeneral = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaEvaluacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgresosAlumno", x => x.IdProgreso);
                    table.ForeignKey(
                        name: "FK_ProgresosAlumno_AspNetUsers_IdAlumno",
                        column: x => x.IdAlumno,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgresosAlumno_AspNetUsers_IdProfesor",
                        column: x => x.IdProfesor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RachasAsistencia",
                columns: table => new
                {
                    IdRacha = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlumno = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RachaActual = table.Column<int>(type: "int", nullable: false),
                    RachaMaxima = table.Column<int>(type: "int", nullable: false),
                    TotalClasesAsistidas = table.Column<int>(type: "int", nullable: false),
                    UltimaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RachasAsistencia", x => x.IdRacha);
                    table.ForeignKey(
                        name: "FK_RachasAsistencia_AspNetUsers_IdAlumno",
                        column: x => x.IdAlumno,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZonasCobertura",
                columns: table => new
                {
                    IdZona = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CostoAdicional = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonasCobertura", x => x.IdZona);
                });

            migrationBuilder.CreateTable(
                name: "Enfrentamientos",
                columns: table => new
                {
                    IdEnfrentamiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCampeonato = table.Column<int>(type: "int", nullable: false),
                    IdJugador1 = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdJugador2 = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdGanador = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Resultado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaPartido = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdCancha = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enfrentamientos", x => x.IdEnfrentamiento);
                    table.ForeignKey(
                        name: "FK_Enfrentamientos_AspNetUsers_IdGanador",
                        column: x => x.IdGanador,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Enfrentamientos_AspNetUsers_IdJugador1",
                        column: x => x.IdJugador1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Enfrentamientos_AspNetUsers_IdJugador2",
                        column: x => x.IdJugador2,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Enfrentamientos_Campeonatos_IdCampeonato",
                        column: x => x.IdCampeonato,
                        principalTable: "Campeonatos",
                        principalColumn: "IdCampeonato",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enfrentamientos_Canchas_IdCancha",
                        column: x => x.IdCancha,
                        principalTable: "Canchas",
                        principalColumn: "IdCancha");
                });

            migrationBuilder.CreateTable(
                name: "InscripcionesCampeonato",
                columns: table => new
                {
                    IdInscripcion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCampeonato = table.Column<int>(type: "int", nullable: false),
                    IdAlumno = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InscripcionesCampeonato", x => x.IdInscripcion);
                    table.ForeignKey(
                        name: "FK_InscripcionesCampeonato_AspNetUsers_IdAlumno",
                        column: x => x.IdAlumno,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InscripcionesCampeonato_Campeonatos_IdCampeonato",
                        column: x => x.IdCampeonato,
                        principalTable: "Campeonatos",
                        principalColumn: "IdCampeonato",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransaccionesProducto",
                columns: table => new
                {
                    IdTransaccion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    IdAlumno = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaDevolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdPago = table.Column<int>(type: "int", nullable: true),
                    FechaTransaccion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaccionesProducto", x => x.IdTransaccion);
                    table.ForeignKey(
                        name: "FK_TransaccionesProducto_AspNetUsers_IdAlumno",
                        column: x => x.IdAlumno,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransaccionesProducto_Pagos_IdPago",
                        column: x => x.IdPago,
                        principalTable: "Pagos",
                        principalColumn: "IdPago");
                    table.ForeignKey(
                        name: "FK_TransaccionesProducto_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UbicacionesAlumno",
                columns: table => new
                {
                    IdUbicacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlumno = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DireccionCompleta = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Latitud = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitud = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IdZona = table.Column<int>(type: "int", nullable: true),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UbicacionesAlumno", x => x.IdUbicacion);
                    table.ForeignKey(
                        name: "FK_UbicacionesAlumno_AspNetUsers_IdAlumno",
                        column: x => x.IdAlumno,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UbicacionesAlumno_ZonasCobertura_IdZona",
                        column: x => x.IdZona,
                        principalTable: "ZonasCobertura",
                        principalColumn: "IdZona");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertasAbandono_IdAlumno",
                table: "AlertasAbandono",
                column: "IdAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultasChatbot_IdUsuario",
                table: "ConsultasChatbot",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_EncuestasSatisfaccion_IdAlumno",
                table: "EncuestasSatisfaccion",
                column: "IdAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_EncuestasSatisfaccion_IdReserva",
                table: "EncuestasSatisfaccion",
                column: "IdReserva");

            migrationBuilder.CreateIndex(
                name: "IX_Enfrentamientos_IdCampeonato",
                table: "Enfrentamientos",
                column: "IdCampeonato");

            migrationBuilder.CreateIndex(
                name: "IX_Enfrentamientos_IdCancha",
                table: "Enfrentamientos",
                column: "IdCancha");

            migrationBuilder.CreateIndex(
                name: "IX_Enfrentamientos_IdGanador",
                table: "Enfrentamientos",
                column: "IdGanador");

            migrationBuilder.CreateIndex(
                name: "IX_Enfrentamientos_IdJugador1",
                table: "Enfrentamientos",
                column: "IdJugador1");

            migrationBuilder.CreateIndex(
                name: "IX_Enfrentamientos_IdJugador2",
                table: "Enfrentamientos",
                column: "IdJugador2");

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesCampeonato_IdAlumno",
                table: "InscripcionesCampeonato",
                column: "IdAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesCampeonato_IdCampeonato",
                table: "InscripcionesCampeonato",
                column: "IdCampeonato");

            migrationBuilder.CreateIndex(
                name: "IX_LogrosAlumno_IdAlumno",
                table: "LogrosAlumno",
                column: "IdAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosAlumno_IdAlumno",
                table: "ProgresosAlumno",
                column: "IdAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosAlumno_IdProfesor",
                table: "ProgresosAlumno",
                column: "IdProfesor");

            migrationBuilder.CreateIndex(
                name: "IX_RachasAsistencia_IdAlumno",
                table: "RachasAsistencia",
                column: "IdAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesProducto_IdAlumno",
                table: "TransaccionesProducto",
                column: "IdAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesProducto_IdPago",
                table: "TransaccionesProducto",
                column: "IdPago");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesProducto_IdProducto",
                table: "TransaccionesProducto",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_UbicacionesAlumno_IdAlumno",
                table: "UbicacionesAlumno",
                column: "IdAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_UbicacionesAlumno_IdZona",
                table: "UbicacionesAlumno",
                column: "IdZona");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertasAbandono");

            migrationBuilder.DropTable(
                name: "ConsultasChatbot");

            migrationBuilder.DropTable(
                name: "EncuestasSatisfaccion");

            migrationBuilder.DropTable(
                name: "Enfrentamientos");

            migrationBuilder.DropTable(
                name: "InscripcionesCampeonato");

            migrationBuilder.DropTable(
                name: "LogrosAlumno");

            migrationBuilder.DropTable(
                name: "PreguntasFrecuentes");

            migrationBuilder.DropTable(
                name: "ProgresosAlumno");

            migrationBuilder.DropTable(
                name: "RachasAsistencia");

            migrationBuilder.DropTable(
                name: "TransaccionesProducto");

            migrationBuilder.DropTable(
                name: "UbicacionesAlumno");

            migrationBuilder.DropTable(
                name: "Campeonatos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "ZonasCobertura");
        }
    }
}
