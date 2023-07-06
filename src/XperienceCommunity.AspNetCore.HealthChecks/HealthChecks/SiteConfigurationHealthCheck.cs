using System.Data;
using CMS.SiteProvider;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class SiteConfigurationHealthCheck: IHealthCheck
    {
        private readonly ISiteInfoProvider _siteInfoProvider;

        public SiteConfigurationHealthCheck(ISiteInfoProvider siteInfoProvider)
        {
            _siteInfoProvider = siteInfoProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var siteResult = await _siteInfoProvider.Get()
                    .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                    .ConfigureAwait(false);
                
                var sites = siteResult.ToList();

                return !sites.Any() ? new HealthCheckResult(HealthStatus.Unhealthy, "There are no sites configured.") : new HealthCheckResult(HealthStatus.Healthy, "Sites have been added to the CMS.");
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, e.Message, e);
            }
        }
    }
}
