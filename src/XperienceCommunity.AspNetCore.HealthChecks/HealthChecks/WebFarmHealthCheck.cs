using CMS.DataEngine;
using CMS.WebFarmSync;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Web Farm Health Check
    /// </summary>
    public sealed class WebFarmHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (!CMSApplication.ApplicationInitialized.HasValue)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Application is not Initialized."));
            }
            
            var webFarmServers = WebFarmContext.EnabledServers;

            if (webFarmServers == null || webFarmServers.Count == 0)
            {
                return Task.FromResult(HealthCheckResult.Degraded("No Web Farm Info Returned"));
            }

            foreach (var server in webFarmServers)
            {
                if (server.Status == WebFarmServerStatusEnum.NotResponding)
                {
                    return Task.FromResult(HealthCheckResult.Degraded($"Server {server.ServerName} is not responding."));
                }
            }

            // If all servers are running, return a healthy status
            return Task.FromResult(HealthCheckResult.Healthy("All servers in the web farm are running."));
        }
    }
}
