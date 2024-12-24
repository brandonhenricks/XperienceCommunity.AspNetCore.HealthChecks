using System.Collections.ObjectModel;
using CMS.DataEngine;
using CMS.Search;
using CMS.SiteProvider;
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

        private static readonly string[] s_columnNames =
        [
            nameof(SearchTaskInfo.SearchTaskErrorMessage),
            nameof(SearchTaskInfo.SearchTaskID)
        ];

        public LocalSearchTaskHealthCheck(ISearchTaskInfoProvider searchTaskInfoProvider)
        {
            _searchTaskInfoProvider = searchTaskInfoProvider ?? throw new ArgumentNullException(nameof(searchTaskInfoProvider));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!CMSApplication.ApplicationInitialized.HasValue)
            {
                return HealthCheckResult.Degraded();
            }
            try
            {
                var searchTasks = await GetDataForTypeAsync(cancellationToken);

                if (searchTasks.Count == 0)
                {
                    return HealthCheckResult.Healthy();
                }

                return GetHealthCheckResult(context, "Local Search Tasks Contain Errors.", data: GetErrorData(searchTasks));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        protected override IEnumerable<SearchTaskInfo> GetDataForType()
        {
            var query = _searchTaskInfoProvider.Get()
                    .Columns(s_columnNames)
                    .WhereNotNullOrEmpty(nameof(SearchTaskInfo.SearchTaskErrorMessage))
                    .OnSite(SiteContext.CurrentSiteID);

            return query.ToList();
        }

        protected override async Task<List<SearchTaskInfo>> GetDataForTypeAsync(CancellationToken cancellationToken = default)
        {
            using (new CMSConnectionScope(true))
            {
                var query = _searchTaskInfoProvider.Get()
                    .Columns(s_columnNames)
                    .WhereNotNullOrEmpty(nameof(SearchTaskInfo.SearchTaskErrorMessage))
                    .OnSite(SiteContext.CurrentSiteID)
                    .TopN(100);

                return await query.ToListAsync(cancellationToken: cancellationToken);
            }
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<SearchTaskInfo> objects)
        {
            var dictionary = objects.ToDictionary<SearchTaskInfo, string, object>(searchTask => searchTask.SearchTaskID.ToString(), searchTask => searchTask.SearchTaskErrorMessage);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
