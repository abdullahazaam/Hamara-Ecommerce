using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HamaraCommerce.Data;

namespace HamaraCommerce.Services
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _context;

        public DatabaseHealthCheck(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                bool canConnect = await _context.Database.CanConnectAsync(cancellationToken);
                if (canConnect)
                {
                    return HealthCheckResult.Healthy("Database connection is healthy and responsive.");
                }
                return HealthCheckResult.Unhealthy("Database connection check returned false.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database connection failed.", ex);
            }
        }
    }
}
