using System.Collections.ObjectModel;
using System.Data;
using CMS.Search;
using CMS.Search.Azure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class AzureSearchTaskHealthCheck : IHealthCheck
    {
        private readonly ISearchTaskAzureInfoProvider _searchTaskAzureInfoProvider;

        public AzureSearchTaskHealthCheck(ISearchTaskAzureInfoProvider searchTaskAzureInfoProvider)
        {
            _searchTaskAzureInfoProvider = searchTaskAzureInfoProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var result = new HealthCheckResult(HealthStatus.Healthy);

            try
            {
                var queryResult = await _searchTaskAzureInfoProvider
                    .Get()
                    .WhereNotNull(nameof(SearchTaskAzureInfo.SearchTaskAzureErrorMessage))
                    .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                    .ConfigureAwait(false);

                var searchTasks = queryResult.ToList();

                if (searchTasks.Any())
                {
                    var healthResultData = GetData(searchTasks);

                    result = new HealthCheckResult(HealthStatus.Degraded, "Azure Search Tasks Contain Errors.",
                        data: healthResultData);
                }

                return result;
            }
            catch (Exception e)
            {
                result = new HealthCheckResult(HealthStatus.Unhealthy, e.Message, e);

                return result;
            }
        }

        private IReadOnlyDictionary<string, object> GetData(IEnumerable<SearchTaskAzureInfo> objects)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var searchTask in objects)
            {
                dictionary.Add(searchTask.SearchTaskAzureID.ToString(), searchTask.SearchTaskAzureErrorMessage);
            }

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
