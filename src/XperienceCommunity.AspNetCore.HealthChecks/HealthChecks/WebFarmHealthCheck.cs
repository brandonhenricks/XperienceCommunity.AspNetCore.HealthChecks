using CMS.WebFarmSync;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class WebFarmHealthCheck : IHealthCheck
    {
        private readonly IWebFarmServerInfoProvider _webFarmServerInfoProvider;

        public WebFarmHealthCheck(IWebFarmServerInfoProvider webFarmServerInfoProvider)
        {
            _webFarmServerInfoProvider = webFarmServerInfoProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            // Get all servers in the web farm
            try
            {
                var servers = _webFarmServerInfoProvider.Get().ToList();

                // If there are no servers, something might be wrong
                if (!servers.Any())
                {
                    return new HealthCheckResult(HealthStatus.Unhealthy, "No servers found in the web farm.");
                }

                // Check the status of each server
                foreach (var server in servers)
                {
                    if (server.Status == WebFarmServerStatusEnum.NotResponding)
                    {
                        return new HealthCheckResult(HealthStatus.Degraded,
                            $"Server {server.ServerName} is not responding.");
                    }
                }

                // If all servers are running, return a healthy status
                return new HealthCheckResult(HealthStatus.Healthy, "All servers in the web farm are running.");
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "An exception was thrown.", e);
            }
        }
    }
}
