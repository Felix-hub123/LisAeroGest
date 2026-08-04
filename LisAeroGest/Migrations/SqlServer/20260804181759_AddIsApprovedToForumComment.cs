using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LisAeroGest.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddIsApprovedToForumComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketsTemp");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReservationExpiresAt",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "ForumComments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationExpiresAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "ForumComments");

            migrationBuilder.CreateTable(
                name: "TicketsTemp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlightId = table.Column<int>(type: "int", nullable: false),
                    PassengerId = table.Column<int>(type: "int", nullable: false),
                    SeatId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraLuggage = table.Column<bool>(type: "bit", nullable: false),
                    MealIncluded = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WasDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketsTemp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketsTemp_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketsTemp_Flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "Flights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketsTemp_Passengers_PassengerId",
                        column: x => x.PassengerId,
                        principalTable: "Passengers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketsTemp_Seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTemp_CreatedByUserId",
                table: "TicketsTemp",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTemp_FlightId",
                table: "TicketsTemp",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTemp_PassengerId",
                table: "TicketsTemp",
                column: "PassengerId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTemp_SeatId",
                table: "TicketsTemp",
                column: "SeatId");
        }
    }
}
