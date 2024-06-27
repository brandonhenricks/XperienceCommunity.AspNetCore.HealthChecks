using System.Collections.ObjectModel;
using System.Data;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Synchronization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.AspNetCore.HealthChecks.Extensions;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class StagingTaskHealthCheck : BaseKenticoHealthCheck<StagingTaskInfo>, IHealthCheck
    {
        private readonly IStagingTaskInfoProvider _stagingTaskInfoProvider;
        private readonly IProgressiveCache _cache;
        private readonly ISiteService _siteService;
        private readonly ISynchronizationInfoProvider _synchronizationInfoProvider;

        public StagingTaskHealthCheck(IStagingTaskInfoProvider stagingTaskInfoProvider, IProgressiveCache progressiveCache, ISiteService siteService, ISynchronizationInfoProvider synchronizationInfoProvider)
        {
            _stagingTaskInfoProvider =
                stagingTaskInfoProvider ?? throw new ArgumentNullException(nameof(stagingTaskInfoProvider));
            _cache = progressiveCache;
            _siteService = siteService;
            _synchronizationInfoProvider = synchronizationInfoProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!CMSApplication.ApplicationInitialized.HasValue)
            {
                return HealthCheckResult.Healthy();
            }
            
            try
            {
                var stagingTasks = await GetDataForTypeAsync(cancellationToken);

                if (stagingTasks.Count == 0)
                {
                    return HealthCheckResult.Healthy("No Staging Tasks Found.");
                }

                var stagingTaskIdList = stagingTasks
                    .Where(s => s.TaskSiteID == _siteService.CurrentSite.SiteID)
                    .Select(x => x.TaskID)
                    .ToList();

                var syncData = await _cache.LoadAsync(async cs =>
                        {
                            var results = await _synchronizationInfoProvider.Get()
                                .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                                .ConfigureAwait(false);

                            cs.CacheDependency =
                                CacheHelper.GetCacheDependency($"{SynchronizationInfo.OBJECT_TYPE}|all");

                            return results.ToList();
                        },
                        new CacheSettings(TimeSpan.FromMinutes(10).TotalMinutes,
                            $"apphealth|{SynchronizationInfo.OBJECT_TYPE}"))
                    .ConfigureAwait(false);

                //var syncTasks = syncData;//.ToList();

                if (syncData.Count == 0)
                {
                    return HealthCheckResult.Healthy("No Synchronization Tasks Found.");
                }

                var syncErrorTasks = syncData
                    .Where(s => stagingTaskIdList.Contains(s.SynchronizationTaskID) &&
                                !string.IsNullOrWhiteSpace(s.SynchronizationErrorMessage))
                    .ToList();

                return syncErrorTasks.Count == 0 ? HealthCheckResult.Healthy("No Synchronization Tasks Contain Errors.") : HealthCheckResult.Degraded("Failed Staging Tasks Found", null, GetData(syncErrorTasks));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        private static IReadOnlyDictionary<string, object> GetData(IEnumerable<SynchronizationInfo> objects)
        {
            var dictionary = objects.ToDictionary<SynchronizationInfo, string, object>(searchTask => searchTask.SynchronizationTaskID.ToString(), searchTask => searchTask.SynchronizationErrorMessage);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }

        protected override IEnumerable<StagingTaskInfo> GetDataForType()
        {
            throw new NotImplementedException();
        }

        protected override async Task<List<StagingTaskInfo>> GetDataForTypeAsync(CancellationToken cancellationToken = default)
        {
            using (new CMSConnectionScope(true))
            {
                var results = _stagingTaskInfoProvider
                    .Get()
                    .OnSite(_siteService.CurrentSite.SiteID);

                return await results.ToListAsync(cancellationToken: cancellationToken);
            }
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<StagingTaskInfo> objects)
        {
            throw new NotImplementedException();
        }
    }
}
