using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace HamaraCommerce.Migrations
{
    public partial class RepairCheckoutOutboxAndLifecycle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Do not silently delete historical financial records to force a constraint through.
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM CouponRedemptions GROUP BY OrderId,CouponCode HAVING COUNT(*)>1) THROW 51000, 'Duplicate coupon redemption records require reconciliation before this migration.', 1;");
            migrationBuilder.AddColumn<string>("CartSnapshotJson", "CheckoutIdempotencyRecords", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<int>("HashVersion", "CheckoutIdempotencyRecords", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<byte[]>("RowVersion", "CheckoutIdempotencyRecords", type: "rowversion", rowVersion: true, nullable: false);
            migrationBuilder.AddColumn<string>("LockToken", "EmailOutboxMessages", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<System.DateTime>("LockExpiresAt", "EmailOutboxMessages", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<System.DateTime>("DeliveredAt", "Orders", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<System.DateTime>("ReturnRequestedAt", "Orders", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<string>("ReturnReason", "Orders", type: "nvarchar(max)", nullable: true);
            migrationBuilder.CreateIndex("IX_CouponRedemptions_OrderId_CouponCode", "CouponRedemptions", new[] { "OrderId", "CouponCode" }, unique: true);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_CouponRedemptions_OrderId_CouponCode", "CouponRedemptions");
            migrationBuilder.DropColumn("CartSnapshotJson", "CheckoutIdempotencyRecords");
            migrationBuilder.DropColumn("HashVersion", "CheckoutIdempotencyRecords");
            migrationBuilder.DropColumn("RowVersion", "CheckoutIdempotencyRecords");
            migrationBuilder.DropColumn("LockToken", "EmailOutboxMessages");
            migrationBuilder.DropColumn("LockExpiresAt", "EmailOutboxMessages");
            migrationBuilder.DropColumn("DeliveredAt", "Orders");
            migrationBuilder.DropColumn("ReturnRequestedAt", "Orders");
            migrationBuilder.DropColumn("ReturnReason", "Orders");
        }
    }
}
