using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HamaraCommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSourceMetadataAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageSourceUrl",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PriceCheckedAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceProductUrl",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceRetailer",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageSourceUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PriceCheckedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SourceProductUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SourceRetailer",
                table: "Products");
        }
    }
}
