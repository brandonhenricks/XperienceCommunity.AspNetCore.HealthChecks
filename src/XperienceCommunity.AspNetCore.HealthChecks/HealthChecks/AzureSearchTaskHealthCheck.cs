using System.Collections.ObjectModel;
using CMS.DataEngine;
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

        private static readonly string[] s_columnNames = new[]
        {
            nameof(SearchTaskAzureInfo.SearchTaskAzureID), 
            nameof(SearchTaskAzureInfo.SearchTaskAzureErrorMessage),
            nameof(SearchTaskAzureInfo.SearchTaskAzureType),
            nameof(SearchTaskAzureInfo.SearchTaskAzureAdditionalData),
        };

        public AzureSearchTaskHealthCheck(ISearchTaskAzureInfoProvider searchTaskAzureInfoProvider)
        {
            _searchTaskAzureInfoProvider = searchTaskAzureInfoProvider ?? throw new ArgumentNullException(nameof(searchTaskAzureInfoProvider));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (!CMSApplication.ApplicationInitialized.HasValue)
            {
                return HealthCheckResult.Healthy();
            }

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
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        protected override IEnumerable<SearchTaskAzureInfo> GetDataForType()
        {
            var result = _searchTaskAzureInfoProvider.Get()
                .Columns(s_columnNames)
                .Where(new WhereCondition()
                    .WhereNotNull(nameof(SearchTaskAzureInfo.SearchTaskAzureErrorMessage))
                    .And()
                    .WhereNotEmpty(nameof(SearchTaskAzureInfo.SearchTaskAzureErrorMessage)));

            return result.ToList();
        }

        protected override async Task<List<SearchTaskAzureInfo>> GetDataForTypeAsync(
            CancellationToken cancellationToken = default)
        {
            var query = _searchTaskAzureInfoProvider.Get()
                .Columns(s_columnNames)
                .Where(new WhereCondition()
                    .WhereNotNull(nameof(SearchTaskAzureInfo.SearchTaskAzureErrorMessage))
                    .And()
                    .WhereNotEmpty(nameof(SearchTaskAzureInfo.SearchTaskAzureErrorMessage)));

            return await query.ToListAsync(cancellationToken: cancellationToken);
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<SearchTaskAzureInfo> objects)
        {
            var dictionary = objects.ToDictionary<SearchTaskAzureInfo, string, object>(searchTask => searchTask.SearchTaskAzureID.ToString(), searchTask => searchTask.SearchTaskAzureErrorMessage);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
