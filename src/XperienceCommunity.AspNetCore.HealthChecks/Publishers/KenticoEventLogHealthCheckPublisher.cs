using CMS.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.Publishers
{
    /// <summary>
    /// Represents a health check publisher that logs health check results to the Kentico event log.
    /// </summary>
    public sealed class KenticoEventLogHealthCheckPublisher : IHealthCheckPublisher
    {
        private readonly IEventLogService _eventLogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoEventLogHealthCheckPublisher"/> class.
        /// </summary>
        /// <param name="eventLogService">The event log service used to log health check results.</param>
        public KenticoEventLogHealthCheckPublisher(IEventLogService eventLogService)
        {
            _eventLogService = eventLogService ?? throw new ArgumentNullException(nameof(eventLogService));
        }

        /// <summary>
        /// Publishes the health check report to the Kentico event log.
        /// </summary>
        /// <param name="report">The health check report.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            switch (report.Status)
            {
                case HealthStatus.Healthy:
                    _eventLogService.LogInformation(nameof(KenticoEventLogHealthCheckPublisher), nameof(HealthReport), "Healthy");
                    return Task.CompletedTask;
                case HealthStatus.Degraded:
                    _eventLogService.LogWarning(nameof(KenticoEventLogHealthCheckPublisher), nameof(HealthReport), "Degraded");
                    return Task.CompletedTask;
                case HealthStatus.Unhealthy:
                    _eventLogService.LogError(nameof(KenticoEventLogHealthCheckPublisher), nameof(HealthReport), "Unhealthy");
                    return Task.CompletedTask;
                default:
                    return Task.CompletedTask;
            }
        }
    }
}
