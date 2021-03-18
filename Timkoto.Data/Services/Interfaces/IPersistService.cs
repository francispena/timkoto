using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NHibernate;

namespace Timkoto.Data.Services.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPersistService
    {
        /// <summary>
        /// Persists the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        Task<long> Save<T>(T data);

        /// <summary>
        /// Update the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        Task<bool> Update<T>(T data);

        /// <summary>
        /// Deletes the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        Task<bool> Delete<T>(T data);

        /// <summary>
        /// Finds the one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressionFunc">The expression function.</param>
        /// <returns></returns>
        Task<T> FindOne<T>(Expression<Func<T, bool>> expressionFunc) where T : class;

        /// <summary>
        /// Finds the many.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressionFunc">The expression function.</param>
        /// <returns></returns>
        Task<List<T>> FindMany<T>(Expression<Func<T, bool>> expressionFunc) where T : class;

        /// <summary>
        /// Finds the last.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressionFunc">The expression function.</param>
        /// <param name="sortFunc">The sort function.</param>
        /// <returns></returns>
        Task<T> FindLast<T>(Expression<Func<T, bool>> expressionFunc, Expression<Func<T, object>> sortFunc)
            where T : class;

        /// <summary>
        /// Batches the save.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        Task<bool> BatchSave<T>(List<T> data);

        /// <summary>
        /// Executes the SQL.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <returns></returns>
        Task<bool> ExecuteSql(string sqlStatement);

        /// <summary>
        /// SQLs the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <returns></returns>
        Task<List<T>> SqlQuery<T>(string sqlStatement);

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <returns></returns>
        ISession GetSession();
    }
}