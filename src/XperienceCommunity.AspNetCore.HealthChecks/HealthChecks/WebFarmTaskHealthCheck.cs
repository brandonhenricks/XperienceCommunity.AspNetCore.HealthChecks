using System.Collections.ObjectModel;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.WebFarmSync;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.AspNetCore.HealthChecks.Extensions;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Web Farm Server Task Health Check
    /// </summary>
    public sealed class WebFarmTaskHealthCheck : BaseKenticoHealthCheck<WebFarmServerTaskInfo>, IHealthCheck
    {
        private readonly IWebFarmServerTaskInfoProvider _webFarmTaskInfoProvider;

        private static readonly string[] s_columnNames =
        [
            nameof(WebFarmServerTaskInfo.ErrorMessage),
            nameof(WebFarmServerTaskInfo.TaskID)
        ];
        
        public WebFarmTaskHealthCheck(IWebFarmServerTaskInfoProvider webFarmTaskInfoProvider)
        {
            _webFarmTaskInfoProvider = webFarmTaskInfoProvider ?? throw new ArgumentNullException(nameof(webFarmTaskInfoProvider));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var result = HealthCheckResult.Healthy();

            if (!CMSApplication.ApplicationInitialized.HasValue)
            {
                return result;
            }

            try
            {
                var data = await GetDataForTypeAsync(cancellationToken);

                if (data.Count != 0)
                {
                    result = HealthCheckResult.Degraded("Web Farm Tasks Contain Errors.", null, GetErrorData(data));
                }

                return result;
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        protected override IEnumerable<WebFarmServerTaskInfo> GetDataForType()
        {
            var query = _webFarmTaskInfoProvider
                .Get()
                .Where(new WhereCondition()
                    .WhereNotNull(nameof(WebFarmServerTaskInfo.ErrorMessage))
                    .Or()
                    .WhereNotEmpty(nameof(WebFarmServerTaskInfo.ErrorMessage)))
                .Columns(s_columnNames)
                .OnSite(SiteContext.CurrentSiteID);

            return query.ToList();
        }

        protected override async Task<List<WebFarmServerTaskInfo>> GetDataForTypeAsync(CancellationToken cancellationToken = default)
        {
            var query = _webFarmTaskInfoProvider
                .Get()
                .Where(new WhereCondition()
                    .WhereNotNull(nameof(WebFarmServerTaskInfo.ErrorMessage))
                    .Or()
                    .WhereNotEmpty(nameof(WebFarmServerTaskInfo.ErrorMessage)))
                .Columns(s_columnNames)
                .OnSite(SiteContext.CurrentSiteID);

            return await query.ToListAsync(cancellationToken: cancellationToken);
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<WebFarmServerTaskInfo> objects)
        {
            var dictionary = objects.ToDictionary<WebFarmServerTaskInfo, string, object>(webFarmTask => webFarmTask.TaskID.ToString(), webFarmTask => webFarmTask.ErrorMessage);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
