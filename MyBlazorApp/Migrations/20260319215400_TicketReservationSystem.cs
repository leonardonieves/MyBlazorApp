using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBlazorApp.Migrations
{
    /// <inheritdoc />
    public partial class TicketReservationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_BuyerEmail",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_StripePaymentIntentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Raffles_IsActive",
                table: "Raffles");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Raffles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Raffles");

            migrationBuilder.RenameColumn(
                name: "PrizeDetails",
                table: "Raffles",
                newName: "FullDescription");

            migrationBuilder.RenameColumn(
                name: "IsDrawn",
                table: "Raffles",
                newName: "IsFeatured");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Raffles",
                newName: "PrimaryImageUrl");

            migrationBuilder.AlterColumn<string>(
                name: "StripePaymentIntentId",
                table: "Tickets",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Tickets",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "BuyerEmail",
                table: "Tickets",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DisplayNumber",
                table: "Tickets",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReservationExpiresAt",
                table: "Tickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReservedAt",
                table: "Tickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SoldAt",
                table: "Tickets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "Tickets",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Raffles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxTicketsPerUser",
                table: "Raffles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SalesEndDate",
                table: "Raffles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "SalesStartDate",
                table: "Raffles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "Raffles",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Raffles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StripeProductId",
                table: "Raffles",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RaffleImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaffleId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AltText = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaffleImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaffleImages_Raffles_RaffleId",
                        column: x => x.RaffleId,
                        principalTable: "Raffles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RafflePrizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaffleId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RafflePrizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RafflePrizes_Raffles_RaffleId",
                        column: x => x.RaffleId,
                        principalTable: "Raffles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_RaffleId_Status",
                table: "Tickets",
                columns: new[] { "RaffleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ReservationExpiresAt",
                table: "Tickets",
                column: "ReservationExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Status",
                table: "Tickets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Raffles_IsFeatured",
                table: "Raffles",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Raffles_SalesEndDate",
                table: "Raffles",
                column: "SalesEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Raffles_SalesStartDate",
                table: "Raffles",
                column: "SalesStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Raffles_Status",
                table: "Raffles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RaffleImages_RaffleId",
                table: "RaffleImages",
                column: "RaffleId");

            migrationBuilder.CreateIndex(
                name: "IX_RafflePrizes_RaffleId",
                table: "RafflePrizes",
                column: "RaffleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_UserId",
                table: "Tickets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_UserId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "RaffleImages");

            migrationBuilder.DropTable(
                name: "RafflePrizes");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_RaffleId_Status",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ReservationExpiresAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_Status",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Raffles_IsFeatured",
                table: "Raffles");

            migrationBuilder.DropIndex(
                name: "IX_Raffles_SalesEndDate",
                table: "Raffles");

            migrationBuilder.DropIndex(
                name: "IX_Raffles_SalesStartDate",
                table: "Raffles");

            migrationBuilder.DropIndex(
                name: "IX_Raffles_Status",
                table: "Raffles");

            migrationBuilder.DropColumn(
                name: "DisplayNumber",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ReservationExpiresAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ReservedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SoldAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Raffles");

            migrationBuilder.DropColumn(
                name: "MaxTicketsPerUser",
                table: "Raffles");

            migrationBuilder.DropColumn(
                name: "SalesEndDate",
                table: "Raffles");

            migrationBuilder.DropColumn(
                name: "SalesStartDate",
                table: "Raffles");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "Raffles");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Raffles");

            migrationBuilder.DropColumn(
                name: "StripeProductId",
                table: "Raffles");

            migrationBuilder.RenameColumn(
                name: "PrimaryImageUrl",
                table: "Raffles",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "IsFeatured",
                table: "Raffles",
                newName: "IsDrawn");

            migrationBuilder.RenameColumn(
                name: "FullDescription",
                table: "Raffles",
                newName: "PrizeDetails");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "StripePaymentIntentId",
                keyValue: null,
                column: "StripePaymentIntentId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "StripePaymentIntentId",
                table: "Tickets",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Tickets",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Tickets",
                keyColumn: "BuyerEmail",
                keyValue: null,
                column: "BuyerEmail",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "BuyerEmail",
                table: "Tickets",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Raffles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Raffles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BuyerEmail",
                table: "Tickets",
                column: "BuyerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_StripePaymentIntentId",
                table: "Tickets",
                column: "StripePaymentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_Raffles_IsActive",
                table: "Raffles",
                column: "IsActive");
        }
    }
}
