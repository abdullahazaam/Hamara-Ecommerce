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

        public static ApplicationDbContext CreateSqlServerDbContext(string? dbName = null)
        {
            var dbIdentifier = dbName ?? "HamaraCommerce_ConcurrencyTestDb";
            var connStr = $"Server=(localdb)\\mssqllocaldb;Database={dbIdentifier};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=15";
            var sqlOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connStr)
                .Options;
            var sqlContext = new ApplicationDbContext(sqlOptions);
            
            // Strictly enforce SQL Server: Do NOT silently fall back to InMemory
            sqlContext.Database.EnsureCreated();
            return sqlContext;
        }

        public static string GetSqlServerConnectionString(string? dbName = null)
        {
            var dbIdentifier = dbName ?? "HamaraCommerce_ConcurrencyTestDb";
            return $"Server=(localdb)\\mssqllocaldb;Database={dbIdentifier};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=15";
        }

        public static (ApplicationDbContext context, bool isSqlServer) CreateTestDbContext(string? dbName = null)
        {
            return (CreateSqlServerDbContext(dbName), true);
        }
    }
}
