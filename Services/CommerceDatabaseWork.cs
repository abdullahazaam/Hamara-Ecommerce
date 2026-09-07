using System.Data;
using System.Security.Cryptography;
using System.Text;
using HamaraCommerce.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HamaraCommerce.Services;

/// <summary>Cross-process SQL Server lock, held on a dedicated connection until disposal.
/// Relational production providers other than SQL Server are deliberately rejected.
/// InMemory is supported for unit tests only; it is not concurrency verification.</summary>
public static class CommerceDatabaseWork
{
    public static async Task<IAsyncDisposable?> TryLockAsync(ApplicationDbContext context, string resource,
        CancellationToken cancellationToken = default)
    {
        if (!context.Database.IsRelational()) return new UnitTestLock();
        if (!context.Database.IsSqlServer()) throw new NotSupportedException("Commerce locking requires SQL Server.");
        // No pooled session may retain an application lock after a broken request.
        var builder = new SqlConnectionStringBuilder(context.Database.GetConnectionString()) { Pooling = false };
        var connection = new SqlConnection(builder.ConnectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "DECLARE @result int; EXEC @result=sys.sp_getapplock @Resource=@resource, @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=0; SELECT @result;";
            command.Parameters.AddWithValue("@resource", "HC:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(resource))));
            var result = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            if (result >= 0) return connection;
            await connection.DisposeAsync();
            return null;
        }
        catch { await connection.DisposeAsync(); throw; }
    }

    public static async Task<T> TransactionAsync<T>(ApplicationDbContext context, Func<Task<T>> action)
    {
        if (!context.Database.IsRelational() || context.Database.CurrentTransaction != null)
        {
            var res = await action();
            await context.SaveChangesAsync();
            return res;
        }
        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var result = await action();
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                context.ChangeTracker.Clear();
                throw;
            }
        });
    }

    private sealed class UnitTestLock : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
