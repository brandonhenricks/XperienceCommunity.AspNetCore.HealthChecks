using System.Collections.ObjectModel;
using System.Data;
using CMS.Search;
using CMS.Search.Azure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class LocalSearchTaskHealthCheck: IHealthCheck
    {
        private readonly ISearchTaskInfoProvider _searchTaskInfoProvider;

        public LocalSearchTaskHealthCheck(ISearchTaskInfoProvider searchTaskInfoProvider)
        {
            _searchTaskInfoProvider = searchTaskInfoProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = new HealthCheckResult(HealthStatus.Healthy);

            try
            {
                var queryResult  = await _searchTaskInfoProvider
                    .Get()
                    .WhereNotNull(nameof(SearchTaskInfo.SearchTaskErrorMessage))
                    .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                    .ConfigureAwait(false);

                var searchTasks = queryResult.ToList();

                if (searchTasks.Any())
                {
                    var resultData = GetData(searchTasks);
                    result = new HealthCheckResult(HealthStatus.Degraded, "Local Search Tasks Contain Errors.", data: resultData);
                }

                return result;
            }
            catch (Exception e)
            {
                result = new HealthCheckResult(HealthStatus.Unhealthy, e.Message, e);

                return result;
            }
        }
        private IReadOnlyDictionary<string, object> GetData(IEnumerable<SearchTaskInfo> objects)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var searchTask in objects)
            {
                dictionary.Add(searchTask.SearchTaskID.ToString(), searchTask.SearchTaskErrorMessage);
            }

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
