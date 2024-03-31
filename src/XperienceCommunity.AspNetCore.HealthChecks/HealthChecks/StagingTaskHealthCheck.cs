using System.Collections.ObjectModel;
using System.Data;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Synchronization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class StagingTaskHealthCheck : IHealthCheck
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
            try
            {
                var data = await _cache.LoadAsync(async cs =>
                        {
                            var results = await _stagingTaskInfoProvider.Get()
                                .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                                .ConfigureAwait(false);

                            cs.CacheDependency = CacheHelper.GetCacheDependency($"{StagingTaskInfo.OBJECT_TYPE}|all");

                            return results;
                        },
                        new CacheSettings(TimeSpan.FromMinutes(10).TotalMinutes,
                            $"apphealth|{StagingTaskInfo.OBJECT_TYPE}"))
                    .ConfigureAwait(false);

                var stagingTasks = data.ToList();

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

                var syncTasks = syncData.ToList();

                if (syncTasks.Count == 0)
                {
                    return HealthCheckResult.Healthy("No Synchronization Tasks Found.");
                }

                var syncErrorTasks = syncTasks
                    .Where(s => stagingTaskIdList.Contains(s.SynchronizationTaskID) &&
                                !string.IsNullOrWhiteSpace(s.SynchronizationErrorMessage))
                    .ToList();

                if (syncErrorTasks.Count == 0)
                {
                    return HealthCheckResult.Healthy("No Synchronization Tasks Contain Errors.");
                }

                return HealthCheckResult.Degraded("Failed Staging Tasks Found", null, GetData(syncErrorTasks));
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
            catch (DataClassNotFoundException)
            {
                return HealthCheckResult.Healthy("No Synchronization Tasks");
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }

        private static IReadOnlyDictionary<string, object> GetData(IEnumerable<SynchronizationInfo> objects)
        {
            var dictionary = objects.ToDictionary<SynchronizationInfo, string, object>(searchTask => searchTask.SynchronizationTaskID.ToString(), searchTask => searchTask.SynchronizationErrorMessage);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
