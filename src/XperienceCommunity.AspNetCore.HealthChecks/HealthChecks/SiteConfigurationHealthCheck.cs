using System.Data;
using CMS.Helpers;
using CMS.SiteProvider;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Site Configuration Health Check
    /// </summary>
    /// <remarks>Investigates the Site Info to determine if any sites are configured.</remarks>
    public sealed class SiteConfigurationHealthCheck : IHealthCheck
    {
        private readonly ISiteInfoProvider _siteInfoProvider;
        private readonly IProgressiveCache _cache;

        public SiteConfigurationHealthCheck(ISiteInfoProvider siteInfoProvider, IProgressiveCache cache)
        {
            _siteInfoProvider = siteInfoProvider ?? throw new ArgumentNullException(nameof(siteInfoProvider));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await _cache.LoadAsync(async cs =>
                    {
                        var results = await _siteInfoProvider.Get()
                            .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                            .ConfigureAwait(false);

                        cs.CacheDependency = CacheHelper.GetCacheDependency($"{SiteInfo.OBJECT_TYPE}|all");

                        return results;
                    }, new CacheSettings(TimeSpan.FromMinutes(10).TotalMinutes, $"apphealth|{SiteInfo.OBJECT_TYPE}"))
                    .ConfigureAwait(false);

                var sites = data.ToList();

                return sites.Count == 0 ? new HealthCheckResult(HealthStatus.Unhealthy, "There are no sites configured.") : new HealthCheckResult(HealthStatus.Healthy, "Sites have been added to the CMS.");
            }
            catch (InvalidOperationException ex)
            {
                return HealthCheckResult.Degraded(ex.Message, ex);
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, e.Message, e);
            }
        }
    }
}
