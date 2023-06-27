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
            var tasks = _searchTaskInfoProvider.Get().ToList();

            var taskErrors = tasks.Where(x => !string.IsNullOrWhiteSpace(x.SearchTaskErrorMessage)).ToList();

            if (taskErrors.Any())
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Search Tasks Contain Errors.");
            }

            return new HealthCheckResult(HealthStatus.Healthy, "Search Tasks Healthy.");
        }
    }
}
