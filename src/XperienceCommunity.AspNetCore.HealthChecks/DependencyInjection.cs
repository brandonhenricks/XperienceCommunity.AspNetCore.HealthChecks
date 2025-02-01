using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using XperienceCommunity.AspNetCore.HealthChecks.HealthChecks;
using XperienceCommunity.AspNetCore.HealthChecks.Publishers;

namespace XperienceCommunity.AspNetCore.HealthChecks
{
    /// <summary>
    /// Dependency Injection Helper Methods.
    /// </summary>
    public static class DependencyInjection
    {
        private const string Kentico = "Kentico";
        private static readonly string[] s_tags = [Kentico];

        /// <summary>
        /// Adds Kentico Health Checks to the specified <see cref="IHealthChecksBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the health checks to.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/> instance.</returns>

        [return: NotNull]
        public static IHealthChecksBuilder AddKenticoHealthChecks(this IHealthChecksBuilder builder)
        {
            return builder
                .AddCheck<ApplicationInitializedHealthCheck>("Application Initialized Health Check", failureStatus: HealthStatus.Unhealthy, tags: s_tags)
                .AddCheck<SiteConfigurationHealthCheck>("Site Configuration Health Check", failureStatus: HealthStatus.Unhealthy, tags: s_tags)
                .AddCheck<SitePresentationHealthCheck>("Site Presentation Url Health Check", failureStatus: HealthStatus.Unhealthy, tags: s_tags)
                .AddCheck<EventLogHealthCheck>("Event Log Health Check", failureStatus: HealthStatus.Degraded, tags: s_tags)
                .AddCheck<WebFarmHealthCheck>("Web Farm Health Check", failureStatus: HealthStatus.Degraded, tags: s_tags)
                .AddCheck<AzureSearchTaskHealthCheck>("Azure Search Task Health Checks", failureStatus: HealthStatus.Degraded, tags: s_tags)
                .AddCheck<WebFarmTaskHealthCheck>("Web Farm Task Health Check", failureStatus: HealthStatus.Unhealthy, tags: s_tags)
                .AddCheck<EmailHealthCheck>("Email Queue Health Check", failureStatus: HealthStatus.Unhealthy, tags: s_tags)
                .AddCheck<EmailSettingsHealthCheck>("Email Settings Health Check", failureStatus: HealthStatus.Degraded, tags: s_tags)
                .AddCheck<LocalSearchTaskHealthCheck>("Local Task Health Check", failureStatus: HealthStatus.Degraded, tags: s_tags);
        }

        /// <summary>
        /// Adds Kentico Specific Health Checks to the specified <see cref="IServiceCollection"/>.
        /// Also registers the Health Checks package, and optionally the Event Log Publisher.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the health checks to.</param>
        /// <param name="useEventLogPublisher">Optionally use the Event Log Publisher.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/> instance.</returns>

        [return: NotNull]
        public static IHealthChecksBuilder AddKenticoHealthChecks(this IServiceCollection services, bool useEventLogPublisher = false)
        {
            if (useEventLogPublisher)
            {
                services.Configure<HealthCheckPublisherOptions>(options =>
                {
                    options.Delay = TimeSpan.FromSeconds(15);
                    options.Predicate = healthCheck => healthCheck.Tags.Contains(Kentico);
                });

                services.AddSingleton<IHealthCheckPublisher, KenticoEventLogHealthCheckPublisher>();
            }

            return services
                .AddHealthChecks()
                .AddKenticoHealthChecks();
        }
    }
}
