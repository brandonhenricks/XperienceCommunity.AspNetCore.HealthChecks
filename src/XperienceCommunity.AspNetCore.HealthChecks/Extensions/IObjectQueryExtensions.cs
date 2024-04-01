using System.Data;
using CMS.DataEngine;

namespace XperienceCommunity.AspNetCore.HealthChecks.Extensions
{
    public static class IObjectQueryExtensions
    {
        /// <summary>
        /// Return an IEnumerable of objects from the query.
        /// </summary>
        /// <typeparam name="TQuery"></typeparam>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<List<TObject>> ToListAsync<TQuery, TObject>(this IObjectQuery<TQuery, TObject> query, CancellationToken cancellationToken = default)
        where TQuery : IObjectQuery<TQuery, TObject>
        where TObject : BaseInfo
        {
            ArgumentNullException.ThrowIfNull(query);

            cancellationToken.ThrowIfCancellationRequested();

            var results = await query //.GetTypedQuery()
                //.GetEnumerableResultAsync(CommandBehavior.Default, false, cancellationToken)
                //.GetEnumerableTypedResultAsync(cancellationToken: cancellationToken);
                .GetEnumerableTypedResultAsync(commandBehavior: CommandBehavior.SequentialAccess, true, cancellationToken: cancellationToken);


            return results?.ToList() ?? new List<TObject>(0);
        }
    }
}
