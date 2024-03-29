using System.Collections.ObjectModel;
using System.Data;
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
                        .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                        .ConfigureAwait(false);

                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{EventLogInfo.OBJECT_TYPE}|all");

                    return results;
                }, new CacheSettings(TimeSpan.FromMinutes(10).TotalMinutes, $"apphealth|{EventLogInfo.OBJECT_TYPE}"))
                    .ConfigureAwait(false);

                var eventList = data.ToList();

                var exceptionEvents = eventList.Where(e => e.EventType == "E" && e.EventTime >= DateTime.UtcNow.AddHours(-24)).OrderByDescending(x => x.EventID).ToList();

                if (exceptionEvents.Count >= 25)
                {
                    return HealthCheckResult.Degraded($"There are {exceptionEvents.Count} errors in the event log.", null, GetData(exceptionEvents));
                }

                return HealthCheckResult.Healthy();
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("open DataReader", StringComparison.OrdinalIgnoreCase))
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

        private static IReadOnlyDictionary<string, object> GetData(IEnumerable<EventLogInfo> objects)
        {
            var dictionary = objects.ToDictionary<EventLogInfo, string, object>(e => e.EventID.ToString(), ev => ev.Exception);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
