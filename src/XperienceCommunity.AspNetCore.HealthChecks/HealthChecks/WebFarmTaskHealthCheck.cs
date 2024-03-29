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
            _webFarmTaskInfoProvider = webFarmTaskInfoProvider ?? throw new ArgumentNullException(nameof(webFarmTaskInfoProvider));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var result = HealthCheckResult.Healthy();

            try
            {
                var data = await _cache.LoadAsync(async cacheSettings =>
                    {
                        // Calls an async method that loads the required data
                        var query = await _webFarmTaskInfoProvider
                            .Get()
                            .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                            .ConfigureAwait(false);

                        cacheSettings.CacheDependency = CacheHelper.GetCacheDependency($"{WebFarmServerTaskInfo.OBJECT_TYPE}|all");

                        return query;
                    }, new CacheSettings(TimeSpan.FromMinutes(10).TotalMinutes, $"apphealth|{WebFarmServerTaskInfo.OBJECT_TYPE}"))
                    .ConfigureAwait(false);

                var errorTasks = data.Where(task => !string.IsNullOrEmpty(task.ErrorMessage)).ToList();

                if (errorTasks.Count != 0)
                {
                    result = HealthCheckResult.Degraded("Web Farm Tasks Contain Errors.");
                }

                return result;
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }
    }
}
