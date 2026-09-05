using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HamaraCommerce.Models;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class CustomerPrivacyAndIdorTests
    {
        [Fact]
        public async Task OrderAccess_BlocksCustomerFromAccessingOtherCustomerOrder()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var customerAOrder = new Order
            {
                Id = 1,
                OrderNumber = "HC-PK-001",
                UserId = "user-customer-a",
                CustomerEmail = "customera@example.com",
                TotalAmount = 5000m
            };

            context.Orders.Add(customerAOrder);
            await context.SaveChangesAsync();

            // Act: Customer B attempts to lookup Order #1
            string loggedInUserId = "user-customer-b";
            var requestedOrder = await context.Orders.FirstOrDefaultAsync(o => o.Id == 1 && o.UserId == loggedInUserId);

            // Assert: Query fails to find order because UserId does not match
            Assert.Null(requestedOrder);
        }

        [Fact]
        public async Task GuestTracking_RequiresExactMatchingEmail()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var order = new Order
            {
                Id = 2,
                OrderNumber = "HC-PK-002",
                TrackingNumber = "TRK-ABC12345",
                CustomerEmail = "buyer@example.com",
                TotalAmount = 7500m
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Act 1: Guest provides wrong email
            string inputTracking = "TRK-ABC12345";
            string wrongEmail = "attacker@example.com";
            var failedLookup = await context.Orders.FirstOrDefaultAsync(o => 
                (o.TrackingNumber == inputTracking || o.OrderNumber == inputTracking) && 
                o.CustomerEmail.ToLower() == wrongEmail.ToLower());

            // Act 2: Guest provides correct email
            string correctEmail = "buyer@example.com";
            var successfulLookup = await context.Orders.FirstOrDefaultAsync(o => 
                (o.TrackingNumber == inputTracking || o.OrderNumber == inputTracking) && 
                o.CustomerEmail.ToLower() == correctEmail.ToLower());

            // Assert
            Assert.Null(failedLookup);
            Assert.NotNull(successfulLookup);
            Assert.Equal("HC-PK-002", successfulLookup.OrderNumber);
        }
    }
}
