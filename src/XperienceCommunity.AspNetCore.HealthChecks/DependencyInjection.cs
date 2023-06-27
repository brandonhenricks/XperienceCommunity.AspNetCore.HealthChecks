using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.AspNetCore.HealthChecks.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks
{
    /// <summary>
    /// Dependency Injection Helper Methods.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds Kentico Specific Health Checks
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddKenticoHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<SiteConfigurationHealthCheck>("Site Presentation Url Health Check")
                .AddCheck<SearchTaskHealthCheck>("Search Task Health Check")
                .AddCheck<WebFarmHealthCheck>("Web Farm Health Check");

            return services;
        }
    }
}
