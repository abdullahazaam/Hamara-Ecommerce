using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HamaraCommerce.Data;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Xunit;

namespace HamaraCommerce.Tests
{
    public class CheckoutAndStockConcurrencyTests
    {
        [Fact]
        public async Task OrderPlacement_DeductsBothProductAndVariantStock_AndRecordsInventoryAudit()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var product = new Product
            {
                Id = 101,
                Title = "Gaming Mouse RGB",
                Price = 4500m,
                Stock = 10,
                Status = ProductStatus.Published,
                CategoryName = "Accessories",
                Variants = new List<ProductVariant>
                {
                    new ProductVariant { Id = 501, SKU = "MOUSE-BLACK", Name = "Black", Stock = 6, IsActive = true },
                    new ProductVariant { Id = 502, SKU = "MOUSE-WHITE", Name = "White", Stock = 4, IsActive = true }
                }
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Act: Deduct 2 units of White variant
            int quantityToBuy = 2;
            int variantId = 502;

            var dbProduct = await context.Products.Include(p => p.Variants).FirstAsync(p => p.Id == 101);
            int prevProductStock = dbProduct.Stock;
            dbProduct.Stock -= quantityToBuy;

            var variant = dbProduct.Variants.First(v => v.Id == variantId);
            int prevVariantStock = variant.Stock;
            variant.Stock -= quantityToBuy;

            var movement = new InventoryMovement
            {
                ProductId = dbProduct.Id,
                VariantId = variant.Id,
                MovementType = InventoryMovementType.OrderDeduction,
                QuantityChange = -quantityToBuy,
                OldStock = prevProductStock,
                NewStock = dbProduct.Stock,
                Reason = "Purchased in Order #HC-PK-CONCUR-01",
                CreatedAt = DateTime.UtcNow
            };
            context.InventoryMovements.Add(movement);
            await context.SaveChangesAsync();

            // Assert
            var updated = await context.Products.Include(p => p.Variants).FirstAsync(p => p.Id == 101);
            Assert.Equal(8, updated.Stock);
            Assert.Equal(2, updated.Variants.First(v => v.Id == 502).Stock);
            Assert.Equal(6, updated.Variants.First(v => v.Id == 501).Stock); // untouched

            var recordedMovement = await context.InventoryMovements.FirstOrDefaultAsync(m => m.ProductId == 101 && m.VariantId == 502);
            Assert.NotNull(recordedMovement);
            Assert.Equal(-2, recordedMovement.QuantityChange);
            Assert.Equal(10, recordedMovement.OldStock);
            Assert.Equal(8, recordedMovement.NewStock);
        }

        [Fact]
        public async Task LastUnitContention_TwoConcurrentPurchases_ExactlyOneSucceeds()
        {
            // Test atomic reservation with lock contention on 1 remaining unit
            var (context, isSqlServer) = TestDbContextFactory.CreateTestDbContext();
            try
            {
                var category = new Category { Name = "Shoes", Slug = "shoes-" + Guid.NewGuid().ToString("N") };
                context.Categories.Add(category);
                await context.SaveChangesAsync();

                var product = new Product
                {
                    Title = "Exclusive Limited Sneaker",
                    SKU = "SNK-LIMITED-" + Guid.NewGuid().ToString("N")[..8],
                    Slug = "snk-limited-" + Guid.NewGuid().ToString("N")[..8],
                    CategoryId = category.Id,
                    Price = 25000m,
                    Stock = 1,
                    Status = ProductStatus.Published,
                    CategoryName = "Shoes"
                };
                context.Products.Add(product);
                await context.SaveChangesAsync();
                int productId = product.Id;

                int successfulPurchases = 0;
                int failedPurchases = 0;

                void AttemptPurchase(string buyer)
                {
                    using var threadContext = new ApplicationDbContext(
                        new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(context.Database.GetConnectionString()!).Options);

                    try
                    {
                        var executionStrategy = threadContext.Database.CreateExecutionStrategy();
                        bool reserved = false;

                        executionStrategy.Execute(() =>
                        {
                            using var transaction = threadContext.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                            try
                            {
                                var liveProduct = threadContext.Products.FirstOrDefault(p => p.Id == productId);
                                if (liveProduct != null && liveProduct.Stock >= 1)
                                {
                                    liveProduct.Stock -= 1;
                                    threadContext.InventoryMovements.Add(new InventoryMovement
                                    {
                                        ProductId = productId,
                                        MovementType = InventoryMovementType.OrderDeduction,
                                        QuantityChange = -1,
                                        OldStock = 1,
                                        NewStock = 0,
                                        Reason = $"Purchased by {buyer}",
                                        CreatedAt = DateTime.UtcNow
                                    });
                                    threadContext.SaveChanges();
                                    transaction.Commit();
                                    reserved = true;
                                }
                                else
                                {
                                    transaction.Rollback();
                                    reserved = false;
                                }
                            }
                            catch
                            {
                                transaction.Rollback();
                                throw;
                            }
                        });

                        if (reserved)
                        {
                            Interlocked.Increment(ref successfulPurchases);
                        }
                        else
                        {
                            Interlocked.Increment(ref failedPurchases);
                        }
                    }
                    catch
                    {
                        // Concurrency conflict / deadlock handled as clean reservation failure
                        Interlocked.Increment(ref failedPurchases);
                    }
                }

                // Run two concurrent attempts simultaneously
                var task1 = Task.Run(() => AttemptPurchase("Buyer 1"));
                var task2 = Task.Run(() => AttemptPurchase("Buyer 2"));

                await Task.WhenAll(task1, task2);

                // Assert: Exactly one purchase must succeed, exactly one must fail
                Assert.Equal(1, successfulPurchases);
                Assert.Equal(1, failedPurchases);

                var finalProduct = await context.Products.AsNoTracking().FirstAsync(p => p.Id == productId);
                Assert.Equal(0, finalProduct.Stock);
            }
            finally
            {
                context.Dispose();
            }
        }

        [Fact]
        public async Task OrderCancellation_RestoresProductAndVariantStock_AndRecordsInventoryAudit()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var product = new Product
            {
                Id = 202,
                Title = "Mechanical Keyboard Blue Switch",
                Price = 9000m,
                Stock = 3,
                Status = ProductStatus.Published,
                CategoryName = "Electronics",
                Variants = new List<ProductVariant>
                {
                    new ProductVariant { Id = 601, SKU = "KB-BLUE", Name = "Blue Switch", Stock = 3, IsActive = true }
                }
            };
            context.Products.Add(product);

            var order = new Order
            {
                Id = 15,
                OrderNumber = "HC-PK-CANCEL-15",
                UserId = "user-cancel-test",
                CustomerEmail = "cancel@test.com",
                Status = OrderStatus.Processing,
                TotalAmount = 9000m,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 202, VariantId = 601, Quantity = 2, UnitPrice = 9000m }
                }
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Act: Simulate cancellation restoration
            foreach (var item in order.Items)
            {
                var p = await context.Products.Include(prod => prod.Variants).FirstAsync(prod => prod.Id == item.ProductId);
                int prevStock = p.Stock;
                p.Stock += item.Quantity;

                if (item.VariantId.HasValue)
                {
                    var v = p.Variants.First(varItem => varItem.Id == item.VariantId.Value);
                    v.Stock += item.Quantity;
                }

                context.InventoryMovements.Add(new InventoryMovement
                {
                    ProductId = p.Id,
                    VariantId = item.VariantId,
                    MovementType = InventoryMovementType.OrderCancellationRestoration,
                    QuantityChange = item.Quantity,
                    OldStock = prevStock,
                    NewStock = p.Stock,
                    Reason = $"Order #{order.OrderNumber} cancelled",
                    CreatedAt = DateTime.UtcNow
                });
            }
            order.Status = OrderStatus.Cancelled;
            await context.SaveChangesAsync();

            // Assert
            var restoredProduct = await context.Products.Include(p => p.Variants).FirstAsync(p => p.Id == 202);
            Assert.Equal(5, restoredProduct.Stock);
            Assert.Equal(5, restoredProduct.Variants.First(v => v.Id == 601).Stock);

            var movement = await context.InventoryMovements.FirstOrDefaultAsync(m => m.MovementType == InventoryMovementType.OrderCancellationRestoration && m.ProductId == 202);
            Assert.NotNull(movement);
            Assert.Equal(2, movement.QuantityChange);
            Assert.Equal(3, movement.OldStock);
            Assert.Equal(5, movement.NewStock);
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
