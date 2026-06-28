using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Persistance.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionEntidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TipoEvento = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Juego = table.Column<string>(type: "text", nullable: true),
                    Plataforma = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Dni = table.Column<string>(type: "text", nullable: false),
                    Contrasenya = table.Column<string>(type: "text", nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Rol = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    EventoId = table.Column<int>(type: "integer", nullable: false),
                    ResumenIA = table.Column<string>(type: "text", nullable: true),
                    TipoProyecto = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Events_EventoId",
                        column: x => x.EventoId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MaxVotesPerVoter = table.Column<int>(type: "integer", nullable: false),
                    EstadoComentarios = table.Column<int>(type: "integer", nullable: false),
                    EsVotacionPopular = table.Column<bool>(type: "boolean", nullable: false),
                    CodigoAcceso = table.Column<string>(type: "text", nullable: true),
                    DnisPermitidos = table.Column<List<string>>(type: "text[]", nullable: false),
                    TipoVotacion = table.Column<int>(type: "integer", nullable: false),
                    ValorMaximoNumerico = table.Column<int>(type: "integer", nullable: true),
                    eventoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votaciones_Events_eventoId",
                        column: x => x.eventoId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProyectoConcursante",
                columns: table => new
                {
                    ProyectosParticipadosId = table.Column<int>(type: "integer", nullable: false),
                    UsuariosId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoConcursante", x => new { x.ProyectosParticipadosId, x.UsuariosId });
                    table.ForeignKey(
                        name: "FK_ProyectoConcursante_Projects_ProyectosParticipadosId",
                        column: x => x.ProyectosParticipadosId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProyectoConcursante_Users_UsuariosId",
                        column: x => x.UsuariosId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CriterioVotacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Peso = table.Column<decimal>(type: "numeric", nullable: false),
                    VotacionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriterioVotacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriterioVotacion_Votaciones_VotacionId",
                        column: x => x.VotacionId,
                        principalTable: "Votaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JuradoVotacion",
                columns: table => new
                {
                    JuradosAsignadosId = table.Column<int>(type: "integer", nullable: false),
                    VotacionesParticipadasId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JuradoVotacion", x => new { x.JuradosAsignadosId, x.VotacionesParticipadasId });
                    table.ForeignKey(
                        name: "FK_JuradoVotacion_Users_JuradosAsignadosId",
                        column: x => x.JuradosAsignadosId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JuradoVotacion_Votaciones_VotacionesParticipadasId",
                        column: x => x.VotacionesParticipadasId,
                        principalTable: "Votaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Premios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Emoji = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    VotacionId = table.Column<int>(type: "integer", nullable: false),
                    ProyectoGanadorId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Premios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Premios_Projects_ProyectoGanadorId",
                        column: x => x.ProyectoGanadorId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Premios_Votaciones_VotacionId",
                        column: x => x.VotacionId,
                        principalTable: "Votaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResumenesIA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VotacionId = table.Column<int>(type: "integer", nullable: false),
                    ProyectoId = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResumenesIA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResumenesIA_Projects_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResumenesIA_Votaciones_VotacionId",
                        column: x => x.VotacionId,
                        principalTable: "Votaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FechaVotado = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Valoracion = table.Column<int>(type: "integer", nullable: true),
                    CriterioValoracionesJson = table.Column<string>(type: "text", nullable: true),
                    NombreUsuarioId = table.Column<int>(type: "integer", nullable: true),
                    VotacionId = table.Column<int>(type: "integer", nullable: false),
                    ProyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_Projects_ProyId",
                        column: x => x.ProyId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Votes_Users_NombreUsuarioId",
                        column: x => x.NombreUsuarioId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Votes_Votaciones_VotacionId",
                        column: x => x.VotacionId,
                        principalTable: "Votaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Contenido = table.Column<string>(type: "text", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    VotoId = table.Column<int>(type: "integer", nullable: false),
                    CriterioId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Votes_VotoId",
                        column: x => x.VotoId,
                        principalTable: "Votes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_VotoId",
                table: "Comments",
                column: "VotoId");

            migrationBuilder.CreateIndex(
                name: "IX_CriterioVotacion_VotacionId",
                table: "CriterioVotacion",
                column: "VotacionId");

            migrationBuilder.CreateIndex(
                name: "IX_JuradoVotacion_VotacionesParticipadasId",
                table: "JuradoVotacion",
                column: "VotacionesParticipadasId");

            migrationBuilder.CreateIndex(
                name: "IX_Premios_ProyectoGanadorId",
                table: "Premios",
                column: "ProyectoGanadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Premios_VotacionId",
                table: "Premios",
                column: "VotacionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_EventoId",
                table: "Projects",
                column: "EventoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoConcursante_UsuariosId",
                table: "ProyectoConcursante",
                column: "UsuariosId");

            migrationBuilder.CreateIndex(
                name: "IX_ResumenesIA_ProyectoId",
                table: "ResumenesIA",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_ResumenesIA_VotacionId",
                table: "ResumenesIA",
                column: "VotacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Votaciones_eventoId",
                table: "Votaciones",
                column: "eventoId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_NombreUsuarioId",
                table: "Votes",
                column: "NombreUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_ProyId",
                table: "Votes",
                column: "ProyId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_VotacionId",
                table: "Votes",
                column: "VotacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "CriterioVotacion");

            migrationBuilder.DropTable(
                name: "JuradoVotacion");

            migrationBuilder.DropTable(
                name: "Premios");

            migrationBuilder.DropTable(
                name: "ProyectoConcursante");

            migrationBuilder.DropTable(
                name: "ResumenesIA");

            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Votaciones");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
