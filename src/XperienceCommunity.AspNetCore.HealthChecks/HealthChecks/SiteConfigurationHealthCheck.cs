using CMS.DataEngine;
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
                return HealthCheckResult.Degraded();
            }

            try
            {
                var sites = await GetDataForTypeAsync(cancellationToken);

                return sites.Count == 0 ?
                    GetHealthCheckResult(context, "There are no sites configured.")
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
            using (new CMSConnectionScope(true))
            {
                var query = _siteInfoProvider.Get();
                return await query.ToListAsync(cancellationToken: cancellationToken);
            }
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<SiteInfo> objects)
        {
            throw new NotImplementedException();
        }
    }
}
