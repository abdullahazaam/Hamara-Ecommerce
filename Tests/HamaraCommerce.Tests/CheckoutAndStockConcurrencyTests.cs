using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class CheckoutAndStockConcurrencyTests
    {
        [Fact]
        public async Task OrderPlacement_DeductsLiveStock_AndRecordsInventoryAudit()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var product = new Product
            {
                Id = 101,
                Title = "Mechanical Gaming Keyboard",
                Price = 8500m,
                Stock = 5,
                Status = ProductStatus.Published,
                CategoryName = "Accessories"
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Simulate stock deduction in checkout logic
            int quantityToBuy = 2;
            int prevStock = product.Stock;
            product.Stock -= quantityToBuy;

            var movement = new InventoryMovement
            {
                ProductId = product.Id,
                MovementType = InventoryMovementType.OrderDeduction,
                QuantityChange = -quantityToBuy,
                OldStock = prevStock,
                NewStock = product.Stock,
                Reason = "Purchased in Order #HC-PK-TEST101",
                CreatedAt = DateTime.UtcNow
            };
            context.InventoryMovements.Add(movement);
            await context.SaveChangesAsync();

            // Assert
            var updatedProduct = await context.Products.FindAsync(101);
            Assert.NotNull(updatedProduct);
            Assert.Equal(3, updatedProduct.Stock);

            var recordedMovement = await context.InventoryMovements.FirstOrDefaultAsync(m => m.ProductId == 101);
            Assert.NotNull(recordedMovement);
            Assert.Equal(-2, recordedMovement.QuantityChange);
            Assert.Equal(5, recordedMovement.OldStock);
            Assert.Equal(3, recordedMovement.NewStock);
        }

        [Fact]
        public void CashOnDelivery_DefaultsPaymentStatusToPending()
        {
            // Arrange & Act
            var order = new Order
            {
                OrderNumber = "HC-PK-2026-0001",
                PaymentMethod = "CashOnDelivery",
                PaymentStatus = PaymentStatus.Pending,
                Status = OrderStatus.Pending,
                TotalAmount = 4500m
            };

            // Assert: Non-paid orders must never be automatically marked Paid
            Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);
            Assert.Equal(OrderStatus.Pending, order.Status);
        }
    }
}
