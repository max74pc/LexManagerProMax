using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexManagerProMax.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelliAtti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Categoria = table.Column<int>(type: "int", nullable: false),
                    Contenuto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VersioneCorrente = table.Column<int>(type: "int", nullable: false),
                    IsAttivo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelliAtti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessioniAI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titolo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoRicerca = table.Column<int>(type: "int", nullable: false),
                    DataSessione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FascicoloId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessioniAI", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Soggetti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Cognome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CodiceFiscale = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    DataNascita = table.Column<DateOnly>(type: "date", nullable: true),
                    LuogoNascita = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RagioneSociale = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PartitaIva = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    FormaGiuridica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Pec = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CellulareUno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Via = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Citta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cap = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Provincia = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Nazione = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCliente = table.Column<bool>(type: "bit", nullable: false),
                    IsControparte = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Soggetti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VersioniModello",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModelloAttoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Versione = table.Column<int>(type: "int", nullable: false),
                    Contenuto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersioniModello", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VersioniModello_ModelliAtti_ModelloAttoId",
                        column: x => x.ModelloAttoId,
                        principalTable: "ModelliAtti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessaggiAI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Contenuto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordine = table.Column<int>(type: "int", nullable: false),
                    Ruolo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessioneAIId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessaggiAI", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessaggiAI_SessioniAI_SessioneAIId",
                        column: x => x.SessioneAIId,
                        principalTable: "SessioniAI",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fascicoli",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Oggetto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Stato = table.Column<int>(type: "int", nullable: false),
                    Rito = table.Column<int>(type: "int", nullable: false),
                    DataApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataChiusura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tribunale = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NrRG = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Giudice = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fascicoli", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fascicoli_Soggetti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Soggetti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FascicoloSoggetti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FascicoloId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoggettoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ruolo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FascicoloSoggetti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FascicoloSoggetti_Fascicoli_FascicoloId",
                        column: x => x.FascicoloId,
                        principalTable: "Fascicoli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FascicoloSoggetti_Soggetti_SoggettoId",
                        column: x => x.SoggettoId,
                        principalTable: "Soggetti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fascicoli_ClienteId",
                table: "Fascicoli",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Fascicoli_Numero",
                table: "Fascicoli",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FascicoloSoggetti_FascicoloId",
                table: "FascicoloSoggetti",
                column: "FascicoloId");

            migrationBuilder.CreateIndex(
                name: "IX_FascicoloSoggetti_SoggettoId",
                table: "FascicoloSoggetti",
                column: "SoggettoId");

            migrationBuilder.CreateIndex(
                name: "IX_MessaggiAI_SessioneAIId",
                table: "MessaggiAI",
                column: "SessioneAIId");

            migrationBuilder.CreateIndex(
                name: "IX_Soggetti_CodiceFiscale",
                table: "Soggetti",
                column: "CodiceFiscale",
                unique: true,
                filter: "[CodiceFiscale] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Soggetti_Cognome_Nome",
                table: "Soggetti",
                columns: new[] { "Cognome", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_Soggetti_PartitaIva",
                table: "Soggetti",
                column: "PartitaIva",
                unique: true,
                filter: "[PartitaIva] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VersioniModello_ModelloAttoId",
                table: "VersioniModello",
                column: "ModelloAttoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FascicoloSoggetti");

            migrationBuilder.DropTable(
                name: "MessaggiAI");

            migrationBuilder.DropTable(
                name: "VersioniModello");

            migrationBuilder.DropTable(
                name: "Fascicoli");

            migrationBuilder.DropTable(
                name: "SessioniAI");

            migrationBuilder.DropTable(
                name: "ModelliAtti");

            migrationBuilder.DropTable(
                name: "Soggetti");
        }
    }
}
