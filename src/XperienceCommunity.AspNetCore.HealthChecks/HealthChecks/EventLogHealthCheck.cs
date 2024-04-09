﻿using System.Collections.ObjectModel;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.SiteProvider;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.AspNetCore.HealthChecks.Extensions;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Event Log Health Check
    /// </summary>
    /// <remarks>Investigates the Last 12 Hours of Event Log Entries for Errors.</remarks>
    public sealed class EventLogHealthCheck : BaseKenticoHealthCheck<EventLogInfo>, IHealthCheck
    {
        private readonly IEventLogInfoProvider _eventLogInfoProvider;

        private static readonly string[] s_columnNames =
        [
            nameof(EventLogInfo.EventType),
            nameof(EventLogInfo.Source),
            nameof(EventLogInfo.EventTime),
            nameof(EventLogInfo.EventID),
            nameof(EventLogInfo.SiteID),
            nameof(EventLogInfo.EventDescription)
        ];

        public EventLogHealthCheck(IEventLogInfoProvider eventLogInfoProvider)
        {
            _eventLogInfoProvider = eventLogInfoProvider ?? throw new ArgumentNullException(nameof(eventLogInfoProvider));
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
                var eventList = await GetDataForTypeAsync(cancellationToken);

                var exceptionEvents = eventList
                    .Where(e => !e.Source.Equals(nameof(HealthReport), StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.EventID)
                    .ToList();

                return exceptionEvents.Count >= 25 ? HealthCheckResult.Degraded($"There are {exceptionEvents.Count} errors in the event log.", null, GetErrorData(exceptionEvents)) : HealthCheckResult.Healthy();
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        protected override IEnumerable<EventLogInfo> GetDataForType()
        {
            var query = _eventLogInfoProvider.Get()
                .Where(new WhereCondition()
                    .WhereEquals(nameof(EventLogInfo.EventType), "E")
                    .And()
                    .WhereNotEquals(nameof(EventLogInfo.Source), nameof(HealthReport))
                    .And()
                    .WhereGreaterOrEquals(nameof(EventLogInfo.EventTime), DateTime.UtcNow.AddHours(-12)))
                .Columns(s_columnNames)
                .OnSite(SiteContext.CurrentSiteID);

            return query.ToList();
        }

        protected override async Task<List<EventLogInfo>> GetDataForTypeAsync(CancellationToken cancellationToken = default)
        {
            var query = _eventLogInfoProvider.Get()
                    .Where(new WhereCondition()
                        .WhereEquals(nameof(EventLogInfo.EventType), "E")
                        .And()
                        .WhereNotEquals(nameof(EventLogInfo.Source), nameof(HealthReport))
                        .And()
                        .WhereGreaterOrEquals(nameof(EventLogInfo.EventTime), DateTime.UtcNow.AddHours(-12)))
                    .Columns(s_columnNames)
                    .OnSite(SiteContext.CurrentSiteID);

            return await query.ToListAsync(cancellationToken: cancellationToken);
        }

        protected override IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<EventLogInfo> objects)
        {
            var dictionary = objects.ToDictionary<EventLogInfo, string, object>(e => e.EventID.ToString(), ev => ev.EventDescription);

            return new ReadOnlyDictionary<string, object>(dictionary);
        }
    }
}
