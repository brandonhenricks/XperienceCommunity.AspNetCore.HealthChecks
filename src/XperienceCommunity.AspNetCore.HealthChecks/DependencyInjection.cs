using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.AspNetCore.HealthChecks.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks
{
    /// <summary>
    /// Dependency Injection Helper Methods.
    /// </summary>
    public static class DependencyInjection
    {
        private const string Kentico = "Kentico";
        private static readonly string[] _tags = new[] {Kentico};

        /// <summary>
        /// Adds Kentico Specific Health Checks
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddKenticoHealthChecks(this IServiceCollection services)
        {
            return services
                .AddHealthChecks()
                .AddCheck<SiteConfigurationHealthCheck>("Site Presentation Url Health Check", tags: _tags)
                .AddCheck<EventLogHealthCheck>("Search Task Health Check", tags: _tags)
                .AddCheck<WebFarmHealthCheck>("Web Farm Health Check", tags: _tags)
                .AddCheck<AzureSearchTaskHealthCheck>("Azure Search Task Health Checks", tags: _tags)
                .AddCheck<WebFarmTaskHealthCheck>("Web Farm Task Health Check", tags: _tags)
                .AddCheck<LocalSearchTaskHealthCheck>("Local Task Health Check", tags: _tags);
        }
    }
}
