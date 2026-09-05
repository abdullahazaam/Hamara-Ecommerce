using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HamaraCommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencyStatusAndPaymentRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "CheckoutIdempotencyRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestAccessToken",
                table: "CheckoutIdempotencyRecords",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAt",
                table: "CheckoutIdempotencyRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentAmount",
                table: "CheckoutIdempotencyRecords",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProvider",
                table: "CheckoutIdempotencyRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "CheckoutIdempotencyRecords",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "CheckoutIdempotencyRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CheckoutIdempotencyRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutIdempotencyRecords_Status",
                table: "CheckoutIdempotencyRecords",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CheckoutIdempotencyRecords_Status",
                table: "CheckoutIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "CheckoutIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "GuestAccessToken",
                table: "CheckoutIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "LockedAt",
                table: "CheckoutIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "PaymentAmount",
                table: "CheckoutIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "PaymentProvider",
                table: "CheckoutIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "CheckoutIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "CheckoutIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CheckoutIdempotencyRecords");
        }
    }
}
