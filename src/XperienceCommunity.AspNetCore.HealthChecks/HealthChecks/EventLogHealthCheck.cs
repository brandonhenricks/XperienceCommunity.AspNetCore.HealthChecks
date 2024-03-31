using System.Collections.ObjectModel;
using System.Data;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Search.Azure;
using CMS.SiteProvider;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.AspNetCore.HealthChecks.Extensions;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Event Log Health Check
    /// </summary>
    /// <remarks>Investigates the Last 100 Event Log Entries for Errors.</remarks>
    public sealed class EventLogHealthCheck : BaseKenticoHealthCheck<EventLogInfo>, IHealthCheck
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
                var eventList = (await GetDataForTypeAsync(cancellationToken)).ToList();

                var exceptionEvents = eventList
                    .Where(e => e.EventType == "E"
                                && e.EventTime >= DateTime.UtcNow.AddHours(-24)
                                && !e.Source.Equals(nameof(HealthReport), StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.EventID)
                    .ToList();

                if (exceptionEvents.Count >= 25)
                {
                    return HealthCheckResult.Degraded($"There are {exceptionEvents.Count} errors in the event log.", null, GetErrorData(exceptionEvents));
                }

                return HealthCheckResult.Healthy();
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("open DataReader", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("current state", StringComparison.OrdinalIgnoreCase))
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

        protected override IEnumerable<EventLogInfo> GetDataForType()
        {
            var query = _eventLogInfoProvider.Get()
                .WhereEquals(nameof(EventLogInfo.EventType), "E")
                .WhereNotEquals(nameof(EventLogInfo.Source), nameof(HealthReport));

            return query.ToList();
        }

        protected override async Task<IEnumerable<EventLogInfo>> GetDataForTypeAsync(CancellationToken cancellationToken = default)
        {
            var query = _eventLogInfoProvider.Get()
                .WhereEquals(nameof(EventLogInfo.EventType), "E")
                .WhereNotEquals(nameof(EventLogInfo.Source), nameof(HealthReport));

            return await query.ToListAsync(cancellationToken: cancellationToken);
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<EventLogInfo> objects)
        {
            var dictionary = objects.ToDictionary<EventLogInfo, string, object>(e => e.EventID.ToString(), ev => ev.Exception);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
