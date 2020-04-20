using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AdminUAT.Migrations
{
    public partial class AddSoporte : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Noticia",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Titulo = table.Column<string>(nullable: true),
                    Descripcion = table.Column<string>(nullable: true),
                    Activo = table.Column<bool>(nullable: false),
                    AltaSistema = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Noticia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoSoporte",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(nullable: true),
                    Nota = table.Column<string>(nullable: true),
                    Activo = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoSoporte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrdenSoporte",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Solicitud = table.Column<string>(nullable: true),
                    Atendido = table.Column<bool>(nullable: false),
                    AtendioUsuario = table.Column<string>(nullable: true),
                    FechaAtencion = table.Column<DateTime>(nullable: false),
                    AltaSistema = table.Column<DateTime>(nullable: false),
                    SolicitudCerrada = table.Column<int>(nullable: false),
                    Activo = table.Column<bool>(nullable: false),
                    Usuario = table.Column<string>(nullable: true),
                    TipoSoporteId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenSoporte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenSoporte_TipoSoporte_TipoSoporteId",
                        column: x => x.TipoSoporteId,
                        principalTable: "TipoSoporte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeguimientoSoporte",
                columns: table => new
                {
                    OrdenSoporteId = table.Column<long>(nullable: false),
                    Usuario = table.Column<string>(nullable: false),
                    AltaSistema = table.Column<DateTime>(nullable: false),
                    Comentario = table.Column<string>(nullable: true),
                    Visto = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeguimientoSoporte", x => new { x.OrdenSoporteId, x.Usuario, x.AltaSistema });
                    table.ForeignKey(
                        name: "FK_SeguimientoSoporte_OrdenSoporte_OrdenSoporteId",
                        column: x => x.OrdenSoporteId,
                        principalTable: "OrdenSoporte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenSoporte_TipoSoporteId",
                table: "OrdenSoporte",
                column: "TipoSoporteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Noticia");

            migrationBuilder.DropTable(
                name: "SeguimientoSoporte");

            migrationBuilder.DropTable(
                name: "OrdenSoporte");

            migrationBuilder.DropTable(
                name: "TipoSoporte");
        }
    }
}
