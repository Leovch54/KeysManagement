using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionLlaves.Migrations
{
    /// <inheritdoc />
    public partial class prueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "edificio",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edificio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "materia",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "periodoAcademico",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    fechaInicio = table.Column<DateTime>(type: "DATE", nullable: false),
                    fechaFin = table.Column<DateTime>(type: "DATE", nullable: false),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    creadoModPor = table.Column<int>(type: "int", nullable: false),
                    ultimaMod = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodoAcademico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "persona",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombres = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    primerApellido = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    segundoApellido = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    creadoModPor = table.Column<int>(type: "int", nullable: false),
                    ultimaMod = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persona", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "aula",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    edificioId = table.Column<int>(type: "int", nullable: false),
                    piso = table.Column<int>(type: "int", nullable: true),
                    capacidad = table.Column<int>(type: "int", nullable: true),
                    tieneProyector = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    tieneTv = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    estadoFisico = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "DISPONIBLE"),
                    estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    creadoModPor = table.Column<int>(type: "int", nullable: false),
                    ultimaMod = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aula", x => x.id);
                    table.ForeignKey(
                        name: "FK_aula_edificio_edificioId",
                        column: x => x.edificioId,
                        principalTable: "edificio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    contrasenia = table.Column<byte[]>(type: "VARBINARY(64)", nullable: false),
                    rol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fechaUltimaConexion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    creadoModPor = table.Column<int>(type: "int", nullable: false),
                    ultimaMod = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_persona_id",
                        column: x => x.id,
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "horarioAcademico",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    periodoAcademicoId = table.Column<int>(type: "int", nullable: false),
                    materiaId = table.Column<int>(type: "int", nullable: false),
                    docenteId = table.Column<int>(type: "int", nullable: false),
                    aulaId = table.Column<int>(type: "int", nullable: false),
                    grupo = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    horaInicio = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    horaFin = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    lunes = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    martes = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    miercoles = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    jueves = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    viernes = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    sabado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    estadoHorario = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    creadoModPor = table.Column<int>(type: "int", nullable: false),
                    ultimaMod = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horarioAcademico", x => x.id);
                    table.ForeignKey(
                        name: "FK_horarioAcademico_aula_aulaId",
                        column: x => x.aulaId,
                        principalTable: "aula",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_horarioAcademico_materia_materiaId",
                        column: x => x.materiaId,
                        principalTable: "materia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_horarioAcademico_periodoAcademico_periodoAcademicoId",
                        column: x => x.periodoAcademicoId,
                        principalTable: "periodoAcademico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_horarioAcademico_persona_docenteId",
                        column: x => x.docenteId,
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reserva",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    solicitanteId = table.Column<int>(type: "int", nullable: false),
                    aulaId = table.Column<int>(type: "int", nullable: false),
                    fechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    proposito = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    justificacion = table.Column<string>(type: "VARCHAR(MAX)", maxLength: 2000, nullable: false),
                    estadoReserva = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true, defaultValue: "PENDIENTE"),
                    aprobadaPor = table.Column<int>(type: "int", nullable: true),
                    fechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    motivoRechazo = table.Column<string>(type: "VARCHAR(MAX)", maxLength: 500, nullable: true),
                    estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    creadoModPor = table.Column<int>(type: "int", nullable: false),
                    ultimaMod = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reserva", x => x.id);
                    table.ForeignKey(
                        name: "FK_reserva_aula_aulaId",
                        column: x => x.aulaId,
                        principalTable: "aula",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reserva_usuario_aprobadaPor",
                        column: x => x.aprobadaPor,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reserva_usuario_solicitanteId",
                        column: x => x.solicitanteId,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prestamo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    reservaId = table.Column<int>(type: "int", nullable: true),
                    horarioAcademicoId = table.Column<int>(type: "int", nullable: true),
                    personaId = table.Column<int>(type: "int", nullable: false),
                    aulaId = table.Column<int>(type: "int", nullable: false),
                    fechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fechaFinProgramada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fechaFinReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    tipo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    estadoPrestamo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true, defaultValue: "ACTIVO"),
                    recibidoPor = table.Column<int>(type: "int", nullable: true),
                    estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    creadoModPor = table.Column<int>(type: "int", nullable: false),
                    ultimaMod = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prestamo", x => x.id);
                    table.ForeignKey(
                        name: "FK_prestamo_aula_aulaId",
                        column: x => x.aulaId,
                        principalTable: "aula",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prestamo_horarioAcademico_horarioAcademicoId",
                        column: x => x.horarioAcademicoId,
                        principalTable: "horarioAcademico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prestamo_persona_personaId",
                        column: x => x.personaId,
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prestamo_reserva_reservaId",
                        column: x => x.reservaId,
                        principalTable: "reserva",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prestamo_usuario_recibidoPor",
                        column: x => x.recibidoPor,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aula_codigo",
                table: "aula",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aula_edificioId",
                table: "aula",
                column: "edificioId");

            migrationBuilder.CreateIndex(
                name: "IX_edificio_codigo",
                table: "edificio",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_horarioAcademico_aulaId",
                table: "horarioAcademico",
                column: "aulaId");

            migrationBuilder.CreateIndex(
                name: "IX_horarioAcademico_docenteId",
                table: "horarioAcademico",
                column: "docenteId");

            migrationBuilder.CreateIndex(
                name: "IX_horarioAcademico_materiaId",
                table: "horarioAcademico",
                column: "materiaId");

            migrationBuilder.CreateIndex(
                name: "IX_horarioAcademico_periodoAcademicoId",
                table: "horarioAcademico",
                column: "periodoAcademicoId");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_aulaId",
                table: "prestamo",
                column: "aulaId");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_horarioAcademicoId",
                table: "prestamo",
                column: "horarioAcademicoId");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_personaId",
                table: "prestamo",
                column: "personaId");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_recibidoPor",
                table: "prestamo",
                column: "recibidoPor");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_reservaId",
                table: "prestamo",
                column: "reservaId");

            migrationBuilder.CreateIndex(
                name: "IX_reserva_aprobadaPor",
                table: "reserva",
                column: "aprobadaPor");

            migrationBuilder.CreateIndex(
                name: "IX_reserva_aulaId",
                table: "reserva",
                column: "aulaId");

            migrationBuilder.CreateIndex(
                name: "IX_reserva_solicitanteId",
                table: "reserva",
                column: "solicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prestamo");

            migrationBuilder.DropTable(
                name: "horarioAcademico");

            migrationBuilder.DropTable(
                name: "reserva");

            migrationBuilder.DropTable(
                name: "materia");

            migrationBuilder.DropTable(
                name: "periodoAcademico");

            migrationBuilder.DropTable(
                name: "aula");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "edificio");

            migrationBuilder.DropTable(
                name: "persona");
        }
    }
}
