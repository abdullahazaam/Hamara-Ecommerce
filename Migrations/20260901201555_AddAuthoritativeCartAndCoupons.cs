using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HamaraCommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthoritativeCartAndCoupons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplicableCategoryId",
                table: "Coupons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApplicableProductId",
                table: "Coupons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FreeShipping",
                table: "Coupons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxDiscountAmount",
                table: "Coupons",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PerUserLimit",
                table: "Coupons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Coupons",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CartJson",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_ApplicableCategoryId",
                table: "Coupons",
                column: "ApplicableCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_ApplicableProductId",
                table: "Coupons",
                column: "ApplicableProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_StartDate",
                table: "Coupons",
                column: "StartDate");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_Categories_ApplicableCategoryId",
                table: "Coupons",
                column: "ApplicableCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_Products_ApplicableProductId",
                table: "Coupons",
                column: "ApplicableProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_Categories_ApplicableCategoryId",
                table: "Coupons");

            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_Products_ApplicableProductId",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_ApplicableCategoryId",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_ApplicableProductId",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_StartDate",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "ApplicableCategoryId",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "ApplicableProductId",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "FreeShipping",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "MaxDiscountAmount",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "PerUserLimit",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "CartJson",
                table: "AspNetUsers");
        }
    }
}
