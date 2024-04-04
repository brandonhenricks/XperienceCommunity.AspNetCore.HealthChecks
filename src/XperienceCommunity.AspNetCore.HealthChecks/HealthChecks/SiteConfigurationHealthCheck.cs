using CMS.DataEngine;
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

        public SiteConfigurationHealthCheck(ISiteInfoProvider siteInfoProvider)
        {
            _siteInfoProvider = siteInfoProvider ?? throw new ArgumentNullException(nameof(siteInfoProvider));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!CMSApplication.ApplicationInitialized.HasValue)
            {
                return HealthCheckResult.Healthy();
            }
            
            try
            {
                var sites = await GetDataForTypeAsync(cancellationToken);

                return sites.Count == 0 ?
                    HealthCheckResult.Unhealthy("There are no sites configured.")
                    : HealthCheckResult.Healthy("Sites have been added to the CMS.");
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        protected override IEnumerable<SiteInfo> GetDataForType()
        {
            var query = _siteInfoProvider.Get();
            
            return query.ToList();
        }

        protected override async Task<List<SiteInfo>> GetDataForTypeAsync(CancellationToken cancellationToken = default)
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
