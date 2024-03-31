using System.Collections.ObjectModel;
using System.Data;
using CMS.Helpers;
using CMS.Search;
using CMS.Search.Azure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.AspNetCore.HealthChecks.Extensions;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Azure Search Task Health Check
    /// </summary>
    /// <remarks>Checks the Azure Search Task for any errors.</remarks>
    public sealed class AzureSearchTaskHealthCheck : BaseKenticoHealthCheck<SearchTaskAzureInfo>, IHealthCheck
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
                var searchTasks = await GetDataForTypeAsync(cancellationToken);

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

                return HealthCheckResult.Degraded("Azure Search Tasks Contain Errors.", data: GetErrorData(errorTasks));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("open DataReader", StringComparison.OrdinalIgnoreCase) 
                    || ex.Message.Contains("current state", StringComparison.OrdinalIgnoreCase)
                    || ex.Message.Contains("reader is closed", StringComparison.OrdinalIgnoreCase))
                {
                    return HealthCheckResult.Healthy();
                }

                return HealthCheckResult.Degraded(ex.Message, ex);
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }

        protected override IEnumerable<SearchTaskAzureInfo> GetDataForType()
        {
            var result = _searchTaskAzureInfoProvider.Get()
                .WhereNotEmpty(nameof(SearchTaskAzureInfo.SearchTaskAzureErrorMessage));

            return result.ToList();
        }

        protected override async Task<List<SearchTaskAzureInfo>> GetDataForTypeAsync(
            CancellationToken cancellationToken = default)
        {
            var query = _searchTaskAzureInfoProvider.Get()
                .WhereNotEmpty(nameof(SearchTaskAzureInfo.SearchTaskAzureErrorMessage));

            return await query.ToListAsync(cancellationToken: cancellationToken);
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<SearchTaskAzureInfo> objects)
        {
            var dictionary = objects.ToDictionary<SearchTaskAzureInfo, string, object>(searchTask => searchTask.SearchTaskAzureID.ToString(), searchTask => searchTask.SearchTaskAzureErrorMessage);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
