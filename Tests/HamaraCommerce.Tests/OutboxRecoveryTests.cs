using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HamaraCommerce.Tests;

public class OutboxRecoveryTests
{
    [Fact]
    public async Task SentMessageCannotBeSentAgainThroughSingleDispatch()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var sender = new Mock<IEmailSender>();
        sender.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(EmailSendResult.Succeeded());
        var service = new EmailOutboxService(db, sender.Object, NullLogger<EmailOutboxService>.Instance);
        var message = await service.QueueEmailAsync("buyer@example.com", "Order", "<p>Order</p>");
        Assert.Equal(EmailOutboxStatus.Sent, (await service.DispatchSingleAsync(message.Id))!.Status);
        Assert.Null(await service.DispatchSingleAsync(message.Id));
        sender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BlockedMessageResumesAfterConfigurationIsFixedWithoutExhaustingAttempts()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var sender = new Mock<IEmailSender>();
        sender.SetupSequence(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailSendResult.Blocked("SMTP unavailable"))
            .ReturnsAsync(EmailSendResult.Succeeded());
        var service = new EmailOutboxService(db, sender.Object, NullLogger<EmailOutboxService>.Instance);
        var msg = await service.QueueEmailAsync("buyer@example.com", "Order", "<p>Order</p>");
        await service.ProcessOutboxAsync();
        var blocked = await db.EmailOutboxMessages.AsNoTracking().SingleAsync();
        Assert.Equal(EmailOutboxStatus.Blocked, blocked.Status);
        Assert.Equal(0, blocked.AttemptCount);
        Assert.Equal(0, await service.ProcessOutboxAsync());
        msg.NextAttemptAt = DateTime.UtcNow.AddSeconds(-1);
        await db.SaveChangesAsync();
        await service.ProcessOutboxAsync();
        Assert.Equal(EmailOutboxStatus.Sent, (await db.EmailOutboxMessages.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    public async Task CrashLeftProcessingMessageIsRecoveredButLiveLeaseIsNotStolen()
    {
        using var db = TestDbContextFactory.CreateInMemoryDbContext();
        var sender = new Mock<IEmailSender>();
        sender.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(EmailSendResult.Succeeded());
        var service = new EmailOutboxService(db, sender.Object, NullLogger<EmailOutboxService>.Instance);
        var msg = await service.QueueEmailAsync("buyer@example.com", "Order", "<p>Order</p>");
        msg.Status = EmailOutboxStatus.Processing;
        msg.LockToken = "dead-worker";
        msg.LockExpiresAt = DateTime.UtcNow.AddMinutes(5);
        await db.SaveChangesAsync();
        Assert.Equal(0, await service.ProcessOutboxAsync());
        msg.LockExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();
        Assert.Equal(1, await service.ProcessOutboxAsync());
        Assert.Equal(EmailOutboxStatus.Sent, (await db.EmailOutboxMessages.AsNoTracking().SingleAsync()).Status);
    }
}
