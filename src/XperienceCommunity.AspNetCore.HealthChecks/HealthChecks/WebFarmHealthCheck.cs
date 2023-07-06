using CMS.WebFarmSync;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class WebFarmHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var webFarmServers = WebFarmContext.EnabledServers;

            if (!webFarmServers.Any())
            {
                return Task.FromResult(new HealthCheckResult(HealthStatus.Degraded, "No Web Farm Info Returned"));
            }

            foreach (var server in webFarmServers)
            {
                if (server.Status == WebFarmServerStatusEnum.NotResponding)
                {
                    return Task.FromResult(new HealthCheckResult(HealthStatus.Degraded,
                        $"Server {server.ServerName} is not responding."));
                }
            }

            // If all servers are running, return a healthy status
            return Task.FromResult(new HealthCheckResult(HealthStatus.Healthy,
                "All servers in the web farm are running."));
        }
    }
}
