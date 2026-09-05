using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HamaraCommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPrivacyIdempotencyAndCouponRedemption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GuestAccessExpiry",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestAccessToken",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CheckoutIdempotencyRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CustomerEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RequestHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckoutIdempotencyRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CouponRedemptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    CouponCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CustomerEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRestored = table.Column<bool>(type: "bit", nullable: false),
                    RestoredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RestoreReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponRedemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponRedemptions_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CouponRedemptions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_GuestAccessToken",
                table: "Orders",
                column: "GuestAccessToken");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutIdempotencyRecords_CreatedAt",
                table: "CheckoutIdempotencyRecords",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutIdempotencyRecords_CustomerEmail",
                table: "CheckoutIdempotencyRecords",
                column: "CustomerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutIdempotencyRecords_IdempotencyKey",
                table: "CheckoutIdempotencyRecords",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CouponRedemptions_CouponCode",
                table: "CouponRedemptions",
                column: "CouponCode");

            migrationBuilder.CreateIndex(
                name: "IX_CouponRedemptions_CouponId",
                table: "CouponRedemptions",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponRedemptions_CustomerEmail",
                table: "CouponRedemptions",
                column: "CustomerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_CouponRedemptions_IsRestored",
                table: "CouponRedemptions",
                column: "IsRestored");

            migrationBuilder.CreateIndex(
                name: "IX_CouponRedemptions_OrderId",
                table: "CouponRedemptions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponRedemptions_UserId",
                table: "CouponRedemptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckoutIdempotencyRecords");

            migrationBuilder.DropTable(
                name: "CouponRedemptions");

            migrationBuilder.DropIndex(
                name: "IX_Orders_GuestAccessToken",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GuestAccessExpiry",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GuestAccessToken",
                table: "Orders");
        }
    }
}
