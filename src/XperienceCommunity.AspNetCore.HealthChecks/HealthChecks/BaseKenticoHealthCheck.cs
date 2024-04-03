using CMS.DataEngine;
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
            if (ex is OperationCanceledException oe)
            {
                return HealthCheckResult.Healthy("Operation Cancelled.");
            }
            
            if (ex is InvalidOperationException ioe)
            {
                if (ioe.Message.Contains("open DataReader", StringComparison.OrdinalIgnoreCase)
                    || ioe.Message.Contains("current state", StringComparison.OrdinalIgnoreCase)
                    || ioe.Message.Contains("reader is closed", StringComparison.OrdinalIgnoreCase)
                    || ioe.Message.Contains("connection is closed", StringComparison.OrdinalIgnoreCase))
                {
                    return HealthCheckResult.Healthy(ioe.Message);
                }

                return HealthCheckResult.Degraded(ioe.Message, ioe);
            }

            if (ex is DataClassNotFoundException de)
            {
                return HealthCheckResult.Healthy(de.Message);
            }

            return HealthCheckResult.Unhealthy(ex.Message, ex);
        }
    }
}
