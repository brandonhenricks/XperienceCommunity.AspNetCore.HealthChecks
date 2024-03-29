using System.Data;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Event Log Health Check
    /// </summary>
    /// <remarks>Investigates the Last 100 Event Log Entries for Errors.</remarks>
    public sealed class EventLogHealthCheck : IHealthCheck
    {
        private readonly IEventLogInfoProvider _eventLogInfoProvider;
        private readonly IProgressiveCache _cache;

        public EventLogHealthCheck(IEventLogInfoProvider eventLogInfoProvider, IProgressiveCache cache)
        {
            _eventLogInfoProvider = eventLogInfoProvider ?? throw new ArgumentNullException(nameof(eventLogInfoProvider));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await _cache.LoadAsync(async cs =>
                {
                    var results = await _eventLogInfoProvider
                        .Get()
                        .Where(new WhereCondition(nameof(EventLogInfo.EventType), QueryOperator.Equals, "E"))
                        .And(new WhereCondition(nameof(EventLogInfo.EventTime), QueryOperator.GreaterOrEquals, DateTime.Now.AddDays(-1)))
                        .OrderByDescending(
                            nameof(EventLogInfo.EventID))
                        .TopN(100)
                        .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                        .ConfigureAwait(false);

                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{EventLogInfo.OBJECT_TYPE}|all");

                    return results;
                }, new CacheSettings(TimeSpan.FromMinutes(10).TotalMinutes, $"apphealth|{EventLogInfo.OBJECT_TYPE}"))
                    .ConfigureAwait(false);

                var exceptionEvents = data.ToList();

                if (exceptionEvents.Count >= 25)
                {
                    return new HealthCheckResult(HealthStatus.Degraded,
                        $"There are {exceptionEvents.Count} errors in the event log.");
                }

                return new HealthCheckResult(HealthStatus.Healthy);
            }
            catch (Exception e)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, e.Message, e);
            }
        }
    }
}
