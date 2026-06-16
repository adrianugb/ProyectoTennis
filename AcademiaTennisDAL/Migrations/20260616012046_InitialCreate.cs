using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademiaTennisDAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Bloqueado = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
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
                name: "Canchas",
                columns: table => new
                {
                    IdCancha = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Disponible = table.Column<bool>(type: "bit", nullable: false),
                    EnMantenimiento = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Canchas", x => x.IdCancha);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesNotificacion",
                columns: table => new
                {
                    IdConfiguracion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Evento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesNotificacion", x => x.IdConfiguracion);
                });

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
                    ProductosAplicables = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ofertas", x => x.IdOferta);
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
                name: "Profesores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Especialidad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profesores", x => x.Id);
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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "Notificaciones",
                columns: table => new
                {
                    IdNotificacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Leida = table.Column<bool>(type: "bit", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.IdNotificacion);
                    table.ForeignKey(
                        name: "FK_Notificaciones_AspNetUsers_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    IdPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlumno = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TipoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EsManual = table.Column<bool>(type: "bit", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.IdPago);
                    table.ForeignKey(
                        name: "FK_Pagos_AspNetUsers_IdAlumno",
                        column: x => x.IdAlumno,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreferenciasNotificacion",
                columns: table => new
                {
                    IdPreferencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CanalPreferido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NotificacionesPago = table.Column<bool>(type: "bit", nullable: false),
                    NotificacionesClase = table.Column<bool>(type: "bit", nullable: false),
                    NotificacionesRecordatorio = table.Column<bool>(type: "bit", nullable: false),
                    NotificacionesCampeonato = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferenciasNotificacion", x => x.IdPreferencia);
                    table.ForeignKey(
                        name: "FK_PreferenciasNotificacion_AspNetUsers_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "Reservas",
                columns: table => new
                {
                    IdReserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlumno = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdCancha = table.Column<int>(type: "int", nullable: false),
                    IdProfesor = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FechaReserva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.IdReserva);
                    table.ForeignKey(
                        name: "FK_Reservas_AspNetUsers_IdAlumno",
                        column: x => x.IdAlumno,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservas_AspNetUsers_IdProfesor",
                        column: x => x.IdProfesor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservas_Canchas_IdCancha",
                        column: x => x.IdCancha,
                        principalTable: "Canchas",
                        principalColumn: "IdCancha",
                        onDelete: ReferentialAction.Cascade);
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
                name: "Cursos",
                columns: table => new
                {
                    IdCurso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Nivel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CuposDisponibles = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    IdProfesor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.IdCurso);
                    table.ForeignKey(
                        name: "FK_Cursos_Profesores_IdProfesor",
                        column: x => x.IdProfesor,
                        principalTable: "Profesores",
                        principalColumn: "Id");
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

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    IdFactura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPago = table.Column<int>(type: "int", nullable: false),
                    NumeroFactura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaFactura = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.IdFactura);
                    table.ForeignKey(
                        name: "FK_Facturas_Pagos_IdPago",
                        column: x => x.IdPago,
                        principalTable: "Pagos",
                        principalColumn: "IdPago",
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
                name: "AsistenciasClase",
                columns: table => new
                {
                    IdAsistencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdReserva = table.Column<int>(type: "int", nullable: false),
                    Asistio = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsistenciasClase", x => x.IdAsistencia);
                    table.ForeignKey(
                        name: "FK_AsistenciasClase_Reservas_IdReserva",
                        column: x => x.IdReserva,
                        principalTable: "Reservas",
                        principalColumn: "IdReserva",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "Horarios",
                columns: table => new
                {
                    IdHorario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaSemana = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    IdCurso = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Horarios", x => x.IdHorario);
                    table.ForeignKey(
                        name: "FK_Horarios_Cursos_IdCurso",
                        column: x => x.IdCurso,
                        principalTable: "Cursos",
                        principalColumn: "IdCurso",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matriculas",
                columns: table => new
                {
                    IdMatricula = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAlumno = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdCurso = table.Column<int>(type: "int", nullable: false),
                    FechaMatricula = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matriculas", x => x.IdMatricula);
                    table.ForeignKey(
                        name: "FK_Matriculas_AspNetUsers_IdAlumno",
                        column: x => x.IdAlumno,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Matriculas_Cursos_IdCurso",
                        column: x => x.IdCurso,
                        principalTable: "Cursos",
                        principalColumn: "IdCurso",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertasAbandono_IdAlumno",
                table: "AlertasAbandono",
                column: "IdAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_AsistenciasClase_IdReserva",
                table: "AsistenciasClase",
                column: "IdReserva");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClasesProgramadas_IdCurso",
                table: "ClasesProgramadas",
                column: "IdCurso");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultasChatbot_IdUsuario",
                table: "ConsultasChatbot",
                column: "IdUsuario");

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

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_IdProfesor",
                table: "Cursos",
                column: "IdProfesor");

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
                name: "IX_Facturas_IdPago",
                table: "Facturas",
                column: "IdPago",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Horarios_IdCurso",
                table: "Horarios",
                column: "IdCurso");

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
                name: "IX_Matriculas_IdAlumno",
                table: "Matriculas",
                column: "IdAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_IdCurso",
                table: "Matriculas",
                column: "IdCurso");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_IdUsuario",
                table: "Notificaciones",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_IdAlumno",
                table: "Pagos",
                column: "IdAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_PreferenciasNotificacion_IdUsuario",
                table: "PreferenciasNotificacion",
                column: "IdUsuario");

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
                name: "IX_Reservas_IdAlumno",
                table: "Reservas",
                column: "IdAlumno");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_IdCancha",
                table: "Reservas",
                column: "IdCancha");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_IdProfesor",
                table: "Reservas",
                column: "IdProfesor");

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
                name: "AsistenciasClase");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ClasesProgramadas");

            migrationBuilder.DropTable(
                name: "ConfiguracionesNotificacion");

            migrationBuilder.DropTable(
                name: "ConsultasChatbot");

            migrationBuilder.DropTable(
                name: "CuponUsos");

            migrationBuilder.DropTable(
                name: "EncuestasSatisfaccion");

            migrationBuilder.DropTable(
                name: "Enfrentamientos");

            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropTable(
                name: "Horarios");

            migrationBuilder.DropTable(
                name: "InscripcionesCampeonato");

            migrationBuilder.DropTable(
                name: "LogrosAlumno");

            migrationBuilder.DropTable(
                name: "Matriculas");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "PreferenciasNotificacion");

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
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Cupones");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Campeonatos");

            migrationBuilder.DropTable(
                name: "Cursos");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "ZonasCobertura");

            migrationBuilder.DropTable(
                name: "Ofertas");

            migrationBuilder.DropTable(
                name: "Canchas");

            migrationBuilder.DropTable(
                name: "Profesores");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
