using System.Collections.ObjectModel;
using System.Data;
using CMS.Helpers;
using CMS.Search;
using CMS.Search.Azure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Azure Search Task Health Check
    /// </summary>
    /// <remarks>Checks the Azure Search Task for any errors.</remarks>
    public sealed class AzureSearchTaskHealthCheck : IHealthCheck
    {
        private readonly ISearchTaskAzureInfoProvider _searchTaskAzureInfoProvider;
        private readonly IProgressiveCache _cache;

        public AzureSearchTaskHealthCheck(ISearchTaskAzureInfoProvider searchTaskAzureInfoProvider, IProgressiveCache cache)
        {
            _searchTaskAzureInfoProvider = searchTaskAzureInfoProvider ?? throw new ArgumentNullException(nameof(searchTaskAzureInfoProvider));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Asynchronously loads data and ensures caching
                var data = await _cache.LoadAsync(async cacheSettings =>
                        {
                            // Calls an async method that loads the required data
                            var result = await _searchTaskAzureInfoProvider
                                .Get()
                                .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                                .ConfigureAwait(false);

                            cacheSettings.CacheDependency =
                                CacheHelper.GetCacheDependency($"{SearchTaskAzureInfo.OBJECT_TYPE}|all");

                            return result;
                        },
                        new CacheSettings(TimeSpan.FromMinutes(10).TotalMinutes,
                            $"apphealth|{SearchTaskAzureInfo.OBJECT_TYPE}"))
                    .ConfigureAwait(false);

                var searchTasks = data.ToList();

                if (searchTasks.Count == 0)
                {
                    return HealthCheckResult.Healthy();
                }

                var errorTasks = searchTasks
                    .Where(searchTask => !string.IsNullOrEmpty(searchTask.SearchTaskAzureErrorMessage)).ToList();

                if (errorTasks.Count == 0)
                {
                    return HealthCheckResult.Healthy();
                }

                var healthResultData = GetData(errorTasks);

                return HealthCheckResult.Degraded("Azure Search Tasks Contain Errors.", data: healthResultData);
            }
            catch (InvalidOperationException ex)
            {
                return HealthCheckResult.Degraded(ex.Message, ex);
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }

        private static IReadOnlyDictionary<string, object> GetData(IEnumerable<SearchTaskAzureInfo> objects)
        {
            var dictionary = objects.ToDictionary<SearchTaskAzureInfo, string, object>(searchTask => searchTask.SearchTaskAzureID.ToString(), searchTask => searchTask.SearchTaskAzureErrorMessage);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
