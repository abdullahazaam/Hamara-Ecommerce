using System;
using Microsoft.EntityFrameworkCore;
using HamaraCommerce.Data;

namespace HamaraCommerce.Tests
{
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext CreateInMemoryDbContext(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
