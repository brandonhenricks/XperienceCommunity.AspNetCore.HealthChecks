using CMS.Helpers;
using CMS.SiteProvider;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.AspNetCore.HealthChecks.Extensions;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Site Configuration Health Check
    /// </summary>
    /// <remarks>Investigates the Site Info to determine if any sites are configured.</remarks>
    public sealed class SiteConfigurationHealthCheck : BaseKenticoHealthCheck<SiteInfo>, IHealthCheck
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
                var sites = (await GetDataForTypeAsync(cancellationToken)).ToList();

                return sites.Count == 0 ?
                    HealthCheckResult.Unhealthy("There are no sites configured.")
                    : HealthCheckResult.Healthy("Sites have been added to the CMS.");
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("open DataReader", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("current state", StringComparison.OrdinalIgnoreCase))
                {
                    return HealthCheckResult.Healthy();
                }

                return HealthCheckResult.Degraded(ex.Message, ex);
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, e.Message, e);
            }
        }

        protected override IEnumerable<SiteInfo> GetDataForType()
        {
            var query = _siteInfoProvider.Get();
            
            return query.ToList();
        }

        protected override async Task<IEnumerable<SiteInfo>> GetDataForTypeAsync(CancellationToken cancellationToken = default)
        {
            var query = _siteInfoProvider.Get();
            return await query.ToListAsync(cancellationToken: cancellationToken);
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<SiteInfo> objects)
        {
            throw new NotImplementedException();
        }
    }
}
