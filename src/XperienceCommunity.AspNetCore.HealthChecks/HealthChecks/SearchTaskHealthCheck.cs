using CMS.Search;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class SearchTaskHealthCheck : IHealthCheck
    {
        private readonly ISearchTaskInfoProvider _searchTaskInfoProvider;

        public SearchTaskHealthCheck(ISearchTaskInfoProvider searchTaskInfoProvider)
        {
            _searchTaskInfoProvider = searchTaskInfoProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var tasks = _searchTaskInfoProvider.Get().ToList();

                var taskErrors = tasks.Where(x => !string.IsNullOrWhiteSpace(x.SearchTaskErrorMessage)).ToList();

                return taskErrors.Any() ? new HealthCheckResult(HealthStatus.Unhealthy, "Search Tasks Contain Errors.") : new HealthCheckResult(HealthStatus.Healthy, "Search Tasks Healthy.");
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "An exception was thrown.", e);
            }
        }
    }
}
