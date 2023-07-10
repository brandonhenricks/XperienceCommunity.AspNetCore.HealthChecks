using System.Data;
using CMS.Helpers;
using CMS.WebFarmSync;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Web Farm Server Task Health Check
    /// </summary>
    public sealed class WebFarmTaskHealthCheck : IHealthCheck
    {
        private readonly IWebFarmServerTaskInfoProvider _webFarmTaskInfoProvider;
        private readonly IProgressiveCache _cache;

        public WebFarmTaskHealthCheck(IWebFarmServerTaskInfoProvider webFarmTaskInfoProvider, IProgressiveCache cache)
        {
            _webFarmTaskInfoProvider = webFarmTaskInfoProvider;
            _cache = cache;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = new HealthCheckResult(HealthStatus.Healthy);

            try
            {
                var data = await _cache.LoadAsync(async cacheSettings =>
                    {
                        // Calls an async method that loads the required data
                        var result = await _webFarmTaskInfoProvider
                            .Get()
                            .WhereNotNull(nameof(WebFarmServerTaskInfo.ErrorMessage))
                            .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                            .ConfigureAwait(false);


                        cacheSettings.CacheDependency = CacheHelper.GetCacheDependency($"{WebFarmServerTaskInfo.OBJECT_TYPE}|all");

                        return result;
                    }, new CacheSettings(TimeSpan.FromMinutes(10).TotalMinutes, $"apphealth|{WebFarmServerTaskInfo.OBJECT_TYPE}"))
                    .ConfigureAwait(false);


                if (data.Any())
                {
                    result = new HealthCheckResult(HealthStatus.Degraded, "Web Farm Tasks Contain Errors.");
                }

                return result;
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, e.Message, e);
            }
        }
    }
}
