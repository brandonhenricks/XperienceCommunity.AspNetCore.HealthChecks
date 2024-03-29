using System.Collections.ObjectModel;
using System.Data;
using CMS.Helpers;
using CMS.Search;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Local Search Task Health Check
    /// </summary>
    /// <remarks>Checks the Local Search Tasks to determine if any errors are present.</remarks>
    public sealed class LocalSearchTaskHealthCheck : IHealthCheck
    {
        private readonly ISearchTaskInfoProvider _searchTaskInfoProvider;
        private readonly IProgressiveCache _cache;

        public LocalSearchTaskHealthCheck(ISearchTaskInfoProvider searchTaskInfoProvider, IProgressiveCache cache)
        {
            _searchTaskInfoProvider = searchTaskInfoProvider ?? throw new ArgumentNullException(nameof(searchTaskInfoProvider));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {

            try
            {
                // Asynchronously loads data and ensures caching
                var data = await _cache.LoadAsync(async cacheSettings =>
                    {
                        // Calls an async method that loads the required data
                        var result = await _searchTaskInfoProvider
                            .Get()
                            .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                            .ConfigureAwait(false);

                        cacheSettings.CacheDependency = CacheHelper.GetCacheDependency($"{SearchTaskInfo.OBJECT_TYPE}|all");

                        return result;
                    }, new CacheSettings(TimeSpan.FromMinutes(10).TotalMinutes, $"apphealth|{SearchTaskInfo.OBJECT_TYPE}"))
                    .ConfigureAwait(false);


                var searchTasks = data.ToList();

                if (searchTasks.Count == 0)
                {
                    return HealthCheckResult.Healthy();
                }

                var errorTasks = searchTasks.Where(searchTask => !string.IsNullOrEmpty(searchTask.SearchTaskErrorMessage)).ToList();

                if (errorTasks.Count == 0)
                {
                    return HealthCheckResult.Healthy();
                }

                var resultData = GetData(errorTasks);

                return HealthCheckResult.Degraded("Local Search Tasks Contain Errors.", data: resultData);
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }

        private static IReadOnlyDictionary<string, object> GetData(IEnumerable<SearchTaskInfo> objects)
        {
            var dictionary = objects.ToDictionary<SearchTaskInfo, string, object>(searchTask => searchTask.SearchTaskID.ToString(), searchTask => searchTask.SearchTaskErrorMessage);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
