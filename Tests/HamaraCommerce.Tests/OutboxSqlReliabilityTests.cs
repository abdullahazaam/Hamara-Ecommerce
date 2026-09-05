using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HamaraCommerce.Tests;

// Requires real SQL Server. There is deliberately no InMemory fallback.
public class OutboxSqlReliabilityTests : IDisposable
{
    private readonly string _name = "HamaraCommerce_Outbox_" + Guid.NewGuid().ToString("N");
    private readonly ApplicationDbContext _db;
    public OutboxSqlReliabilityTests() { _db = TestDbContextFactory.CreateSqlServerDbContext(_name); }
    private ApplicationDbContext Open() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer(TestDbContextFactory.GetSqlServerConnectionString(_name)).Options);
    public void Dispose() { _db.Database.EnsureDeleted(); _db.Dispose(); }

    [Fact]
    public async Task TwoWorkersClaimOnlyOneDelivery()
    {
        _db.EmailOutboxMessages.Add(EmailOutboxService.CreateMessage("buyer@example.com", "Order", "body", eventKey: "one"));
        await _db.SaveChangesAsync();
        var count = 0;
        var sender = new Mock<IEmailSender>();
        sender.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>())).Returns(async () =>
            { Interlocked.Increment(ref count); await Task.Delay(100); return EmailSendResult.Succeeded(); });
        using var db1 = Open(); using var db2 = Open();
        var a = new EmailOutboxService(db1, sender.Object, NullLogger<EmailOutboxService>.Instance);
        var b = new EmailOutboxService(db2, sender.Object, NullLogger<EmailOutboxService>.Instance);
        await Task.WhenAll(a.ProcessOutboxAsync(), b.ProcessOutboxAsync());
        Assert.Equal(1, count);
        Assert.Equal(EmailOutboxStatus.Sent, (await _db.EmailOutboxMessages.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    public async Task CouponRestorationRollsBackWithTheOrderCancellation()
    {
        var coupon = new Coupon { Code = "ROLLBACK", UsageCount = 1 };
        var order = new Order { OrderNumber = "ROLLBACK-ORDER", CouponCode = coupon.Code, Status = OrderStatus.Confirmed };
        _db.Coupons.Add(coupon); _db.Orders.Add(order); await _db.SaveChangesAsync();
        _db.CouponRedemptions.Add(new CouponRedemption { CouponId = coupon.Id, OrderId = order.Id, CouponCode = coupon.Code });
        await _db.SaveChangesAsync();
        await using (var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
        {
            order.Status = OrderStatus.Cancelled;
            var pricing = new PricingService(_db, new ShippingTaxService(), NullLogger<PricingService>.Instance);
            Assert.True(await pricing.RestoreCouponRedemptionAsync(order));
            await tx.RollbackAsync(); // Simulate failure after restoration but before cancellation commits.
        }
        using var check = Open();
        Assert.Equal(1, (await check.Coupons.SingleAsync()).UsageCount);
        Assert.False((await check.CouponRedemptions.SingleAsync()).IsRestored);
        Assert.Equal(OrderStatus.Confirmed, (await check.Orders.SingleAsync()).Status);
    }
}
