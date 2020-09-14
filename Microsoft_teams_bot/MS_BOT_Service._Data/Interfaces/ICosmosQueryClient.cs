using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MS.GTA.BOTService.Data.Interfaces
{
    public interface ICosmosQueryClient
    {
        /// <summary>The get first or default.</summary>
        /// <param name="expression">The expression.</param>
        /// <param name="feedOptions">The feed options.</param>
        /// <typeparam name="T">The generic type T</typeparam>
        /// <returns>The <see cref="Task"/>.</returns>
        Task<T> GetFirstOrDefault<T>(Expression<Func<T, bool>> expression, FeedOptions feedOptions = null) where T : class;

        /// <summary>The get.</summary>
        /// <param name="expression">The expression.</param>
        /// <param name="feedOptions">The feed options.</param>
        /// <typeparam name="T">The generic type T</typeparam>
        /// <returns>The <see cref="Task"/>.</returns>
        Task<IEnumerable<T>> Get<T>(Expression<Func<T, bool>> expression, FeedOptions feedOptions = null) where T : class;

        /// <summary>
        /// Gets the with pagination.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="feedOptions">The feed options.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns>
        /// The <see cref="Task" />.
        /// </returns>
        Task<IEnumerable<T>> GetWithPagination<T>(Expression<Func<T, bool>> expression, FeedOptions feedOptions = null, int skip = 0, int take = 0) where T : class;

    }
}
