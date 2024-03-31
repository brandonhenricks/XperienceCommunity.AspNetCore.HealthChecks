using CMS.Base;
using CMS.SiteProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class SitePresentationHealthCheck : BaseKenticoHealthCheck<SiteInfo>, IHealthCheck
    {
        private readonly ISiteService _siteService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISiteInfoProvider _siteInfoProvider;

        public SitePresentationHealthCheck(ISiteService siteService,
            IHttpContextAccessor httpContextAccessor,
            ISiteInfoProvider siteInfoProvider)
        {
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _siteInfoProvider = siteInfoProvider ?? throw new ArgumentNullException(nameof(siteInfoProvider));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var currentSite = await _siteInfoProvider.GetAsync(_siteService.CurrentSite.SiteID, cancellationToken);

            if (currentSite == null)
            {
                return HealthCheckResult.Unhealthy("The current site is not configured.");
            }

            try
            {
                var request = _httpContextAccessor.HttpContext.Request;

                var currentSitePresentationUri = new Uri(currentSite.SitePresentationURL);

                var requestUri = new Uri(request.Scheme + "://" + request.Host.Value);

                if (currentSitePresentationUri.Host == requestUri.Host && currentSitePresentationUri.Scheme == requestUri.Scheme)
                {
                    return HealthCheckResult.Healthy("The current site is configured correctly.");
                }

                var domainAliasList = currentSite.LiveSiteAliases.ToList();

                foreach (var domainAlias in domainAliasList)
                {
                    var domainAliasUri = new Uri(domainAlias.SiteDomainPresentationUrl);

                    if (domainAliasUri.Host == requestUri.Host && domainAliasUri.Scheme == requestUri.Scheme)
                    {
                        return HealthCheckResult.Healthy("The current site is configured correctly.");
                    }
                    
                }
                
                var siteDomainAliasList = currentSite.AdministrationAliases.ToList();

                foreach (var siteDomainAliasInfo in siteDomainAliasList)
                {
                    var domainAliasUri = new Uri(siteDomainAliasInfo.SiteDomainPresentationUrl);

                    if (domainAliasUri.Host == requestUri.Host && domainAliasUri.Scheme == requestUri.Scheme)
                    {
                        return HealthCheckResult.Healthy("The current site is configured correctly.");
                    }
                }
                
                return HealthCheckResult.Unhealthy("The current site is not configured correctly.");
            }
            catch (InvalidOperationException ex)
            {

                if (ex.Message.Contains("open DataReader", StringComparison.OrdinalIgnoreCase))
                {
                    return HealthCheckResult.Healthy();
                }

                return HealthCheckResult.Degraded(ex.Message, ex);
            }
            catch (Exception)
            {
                return HealthCheckResult.Unhealthy("The current site is not configured correctly.");
            }
        }

        protected override IEnumerable<SiteInfo> GetDataForType()
        {
            var query = _siteInfoProvider.Get(_siteService.CurrentSite.SiteID);

            return new List<SiteInfo>() { query };
        }

        protected override Task<IEnumerable<SiteInfo>> GetDataForTypeAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<SiteInfo> objects)
        {
            throw new NotImplementedException();
        }
    }
}
