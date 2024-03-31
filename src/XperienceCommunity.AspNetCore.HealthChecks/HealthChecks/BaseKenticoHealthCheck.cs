using CMS.DataEngine;

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
    }
}
