using System.Data;
using CMS.WebFarmSync;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class WebFarmTaskHealthCheck : IHealthCheck
    {
        private readonly IWebFarmServerTaskInfoProvider _webFarmTaskInfoProvider;

        public WebFarmTaskHealthCheck(IWebFarmServerTaskInfoProvider webFarmTaskInfoProvider)
        {
            _webFarmTaskInfoProvider = webFarmTaskInfoProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = new HealthCheckResult(HealthStatus.Healthy);

            try
            {
                var webFarmTasks = await _webFarmTaskInfoProvider
                    .Get()
                    .WhereNotNull(nameof(WebFarmServerTaskInfo.ErrorMessage))
                    .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                    .ConfigureAwait(false);

                if (webFarmTasks.Any())
                {
                    result = new HealthCheckResult(HealthStatus.Degraded, "Web Farm Tasks Contain Errors.");
                }

                return result;
            }
            catch (Exception e)
            {
                result = new HealthCheckResult(HealthStatus.Unhealthy, e.Message, e);

                return result;
            }
        }
    }
}
