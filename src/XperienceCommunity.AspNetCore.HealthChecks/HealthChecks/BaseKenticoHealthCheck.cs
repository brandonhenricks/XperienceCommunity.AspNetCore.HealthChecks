﻿using CMS.DataEngine;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XperienceCommunity.AspNetCore.HealthChecks.HealthChecks
{
    /// <summary>
    /// Represents a base class for Kentico health checks.
    /// </summary>
    /// <typeparam name="T">The type of the Kentico object.</typeparam>
    public abstract class BaseKenticoHealthCheck<T> where T : BaseInfo
    {
        /// <summary>
        /// Gets the data for the specified Kentico object type.
        /// </summary>
        /// <returns>An enumerable collection of Kentico objects.</returns>
        protected abstract IEnumerable<T> GetDataForType();

        /// <summary>
        /// Gets the data for the specified Kentico object type asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation and contains an enumerable collection of Kentico objects.</returns>
        protected abstract Task<List<T>> GetDataForTypeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the error data for the specified Kentico objects.
        /// </summary>
        /// <param name="objects">The Kentico objects.</param>
        /// <returns>A read-only dictionary containing the error data.</returns>
        protected abstract IReadOnlyDictionary<string, object> GetErrorData(IEnumerable<T> objects);

        /// <summary>
        /// Gets the health check result.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        protected static HealthCheckResult HandleException(Exception ex)
        {
            if (ex is OperationCanceledException or TaskCanceledException)
            {
                return HealthCheckResult.Healthy("Operation Cancelled.");
            }

            return ex switch
            {
                InvalidOperationException ioe when
                    ioe.Message.Contains("open DataReader", StringComparison.OrdinalIgnoreCase) ||
                    ioe.Message.Contains("current state", StringComparison.OrdinalIgnoreCase) ||
                    ioe.Message.Contains("reader is closed", StringComparison.OrdinalIgnoreCase) ||
                    ioe.Message.Contains("connection is closed", StringComparison.OrdinalIgnoreCase) =>
                    HealthCheckResult.Healthy(ioe.Message),
                InvalidOperationException ioe => HealthCheckResult.Degraded(ioe.Message, ioe),
                DataClassNotFoundException de => HealthCheckResult.Healthy(de.Message),
                LinqExpressionCannotBeExecutedException lec => HealthCheckResult.Healthy(lec.Message),
                _ => HealthCheckResult.Unhealthy(ex.Message, ex)
            };
        }

        /// <summary>
        /// Gets the health check result based on the provided context and message.
        /// </summary>
        /// <param name="context">The health check context.</param>
        /// <param name="message">The message to include in the health check result.</param>
        /// <param name="data">Optional. Additional data to include in the health check result.</param>
        /// <returns>The health check result.</returns>
        public HealthCheckResult GetHealthCheckResult(HealthCheckContext context, string message,
            IReadOnlyDictionary<string, object>? data = null)
        {
            var failureStatus = context?.Registration?.FailureStatus ?? HealthStatus.Unhealthy;

            return failureStatus switch
            {
                HealthStatus.Unhealthy => HealthCheckResult.Unhealthy(message, null, data),
                HealthStatus.Degraded => HealthCheckResult.Degraded(message, null, data),
                _ => HealthCheckResult.Unhealthy(message, null, data)
            };
        }
    }
}
