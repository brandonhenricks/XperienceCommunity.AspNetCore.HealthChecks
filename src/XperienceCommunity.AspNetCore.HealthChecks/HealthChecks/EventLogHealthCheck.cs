using System.Data;
using CMS.DataEngine;
using CMS.EventLog;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    public sealed class EventLogHealthCheck : IHealthCheck
    {
        private readonly IEventLogInfoProvider _eventLogInfoProvider;

        public EventLogHealthCheck(IEventLogInfoProvider eventLogInfoProvider)
        {
            _eventLogInfoProvider = eventLogInfoProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var events = await _eventLogInfoProvider
                    .Get()
                    .Where(new WhereCondition(nameof(EventLogInfo.EventType), QueryOperator.Equals,"E"))
                    .And(new WhereCondition(nameof(EventLogInfo.EventTime), QueryOperator.GreaterOrEquals, DateTime.Now.AddDays(-1)))
                    .OrderByDescending(
                        nameof(EventLogInfo.EventID))
                    .TopN(100)
                    .GetEnumerableTypedResultAsync(CommandBehavior.CloseConnection, true, cancellationToken)
                    .ConfigureAwait(false);

                var exceptionEvents = events.ToList();

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
