using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LisAeroGest.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddFlightCommunicationsSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlightCommunications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlightId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SentByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RecipientsCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightCommunications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightCommunications_AspNetUsers_SentByUserId",
                        column: x => x.SentByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlightCommunications_Flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "Flights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlightCommunications_FlightId",
                table: "FlightCommunications",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightCommunications_SentByUserId",
                table: "FlightCommunications",
                column: "SentByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightCommunications");
        }
    }
}
