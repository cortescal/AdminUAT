using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AdminUAT.Migrations.AgendaDb
{
    public partial class Second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dia",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(nullable: true),
                    Numero = table.Column<int>(nullable: false),
                    Activo = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hora",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CampoHora = table.Column<TimeSpan>(nullable: false),
                    Activo = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hora", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoraDia",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MP = table.Column<long>(nullable: false),
                    Activo = table.Column<bool>(nullable: false),
                    AltaSistema = table.Column<DateTime>(nullable: false),
                    HoraId = table.Column<int>(nullable: false),
                    DiaId = table.Column<int>(nullable: false),
                    FechaFinal = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoraDia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoraDia_Dia_DiaId",
                        column: x => x.DiaId,
                        principalTable: "Dia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoraDia_Hora_HoraId",
                        column: x => x.HoraId,
                        principalTable: "Hora",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cita",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Dia = table.Column<DateTime>(nullable: false),
                    Asistencia = table.Column<int>(nullable: false),
                    NumDenuncia = table.Column<long>(nullable: false),
                    Activo = table.Column<bool>(nullable: false),
                    Comentario = table.Column<string>(nullable: true),
                    MP = table.Column<long>(nullable: false),
                    Notificado = table.Column<bool>(nullable: false),
                    SendEmail = table.Column<int>(nullable: false),
                    AltaSistema = table.Column<DateTime>(nullable: false),
                    HoraDiaId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cita_HoraDia_HoraDiaId",
                        column: x => x.HoraDiaId,
                        principalTable: "HoraDia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cita_HoraDiaId",
                table: "Cita",
                column: "HoraDiaId");

            migrationBuilder.CreateIndex(
                name: "IX_HoraDia_DiaId",
                table: "HoraDia",
                column: "DiaId");

            migrationBuilder.CreateIndex(
                name: "IX_HoraDia_HoraId",
                table: "HoraDia",
                column: "HoraId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cita");

            migrationBuilder.DropTable(
                name: "HoraDia");

            migrationBuilder.DropTable(
                name: "Dia");

            migrationBuilder.DropTable(
                name: "Hora");
        }
    }
}
