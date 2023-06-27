using CMS.SiteProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Site Configuration Health Check
    /// </summary>
    /// <remarks>Determines if the Presentation Url Is Configured for the executing context.</remarks>
    public sealed class SiteConfigurationHealthCheck : IHealthCheck
    {
        private readonly ISiteInfoProvider _siteInfoProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SiteConfigurationHealthCheck(ISiteInfoProvider siteInfoProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _siteInfoProvider = siteInfoProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var sites = _siteInfoProvider.Get().ToList();
            var domain = _httpContextAccessor.HttpContext.Request.Host;

            if (!sites.Any())
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "There are no sites configured.");
            }

            if (sites.Count == 1)
            {
                var site = sites.FirstOrDefault();

                if (HasUrlRegistered(site!, domain.Host))
                {
                    return new HealthCheckResult(HealthStatus.Healthy, "Site Registration Found.");
                }

                return new HealthCheckResult(HealthStatus.Unhealthy, "Site Presentation Url is not configured.");
            }

            var urls = sites.Select(GetPresentationUrls).ToList();

            if (urls.Any(x => x.Contains(domain.Host, StringComparer.InvariantCultureIgnoreCase)))
            {
                return new HealthCheckResult(HealthStatus.Healthy, "Site Registration Found.");
            }

            return new HealthCheckResult(HealthStatus.Unhealthy, "Site Presentation Url is not configured.");
        }

        private static HashSet<string> GetPresentationUrls(SiteInfo siteInfo)
        {
            var urlHash = new HashSet<string> {siteInfo.SitePresentationURL};

            foreach (var alias in siteInfo.LiveSiteAliases)
            {
                urlHash.Add(alias.SiteDomainPresentationUrl);
            }

            return urlHash;
        }

        private static bool HasUrlRegistered(SiteInfo siteInfo, string url)
        {
            if (siteInfo.SitePresentationURL.Contains(url, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return siteInfo.LiveSiteAliases.Any(aliasUrl => aliasUrl.SiteDomainPresentationUrl.Contains(url));
        }
    }
}
