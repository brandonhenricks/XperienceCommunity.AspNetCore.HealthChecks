using System.Collections.ObjectModel;
using System.Data;
using CMS.Helpers;
using CMS.Search;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.AspNetCore.HealthChecks.Extensions;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Local Search Task Health Check
    /// </summary>
    /// <remarks>Checks the Local Search Tasks to determine if any errors are present.</remarks>
    public sealed class LocalSearchTaskHealthCheck : BaseKenticoHealthCheck<SearchTaskInfo>, IHealthCheck
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
                var searchTasks = await GetDataForTypeAsync(cancellationToken);

                if (searchTasks.Count == 0)
                {
                    return HealthCheckResult.Healthy();
                }

                return HealthCheckResult.Degraded("Local Search Tasks Contain Errors.", data: GetErrorData(searchTasks));
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

        protected override IEnumerable<SearchTaskInfo> GetDataForType()
        {
            var query = _searchTaskInfoProvider.Get()
                .WhereNotEmpty(nameof(SearchTaskInfo.SearchTaskErrorMessage));
            
            return query.ToList();
        }

        protected override async Task<List<SearchTaskInfo>> GetDataForTypeAsync(CancellationToken cancellationToken = default)
        {
            
            var query = _searchTaskInfoProvider.Get()
                .WhereNotEmpty(nameof(SearchTaskInfo.SearchTaskErrorMessage));

            return await query.ToListAsync(cancellationToken: cancellationToken);
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<SearchTaskInfo> objects)
        {
            var dictionary = objects.ToDictionary<SearchTaskInfo, string, object>(searchTask => searchTask.SearchTaskID.ToString(), searchTask => searchTask.SearchTaskErrorMessage);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
