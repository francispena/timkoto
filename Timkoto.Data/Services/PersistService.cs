using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Timkoto.Data.Services.Interfaces;

namespace Timkoto.Data.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class PersistService : IPersistService
    {

        /// <summary>
        /// The session factory
        /// </summary>
        private readonly ISessionFactory _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistService"/> class.
        /// </summary>
        /// <param name="sessionFactory">The session factory.</param>
        public PersistService(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        /// <summary>
        /// Persists the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public async Task<long> Save<T>(T data)
        {
            var retVal = 0L;
            ITransaction tx = null;
            try
            {
                var dbSession = _sessionFactory.OpenSession();
                tx = dbSession.BeginTransaction();

                var result = await dbSession.SaveAsync(data);
                await tx.CommitAsync();

                if (result != null)
                {
                    retVal = long.Parse(result.ToString());
                }

                dbSession.Close();
                dbSession.Dispose();
            }
            catch (Exception ex)
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }

                if (ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message)
                                              && ex.InnerException.Message.Contains("Duplicate entry"))
                {
                    retVal = -1000;
                }
                else
                {
                    retVal = -1001;
                }
                //await _logger.LogAsync("Error PersistService.Save", ex, null,
                //    new Dictionary<string, object> { { "data", data } });
            }

            return retVal;
        }

        /// <summary>
        /// Update the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public async Task<bool> Update<T>(T data)
        {
            bool retVal;
            ITransaction tx = null;
            try
            {
                var dbSession = _sessionFactory.OpenSession();
                tx = dbSession.BeginTransaction();

                await dbSession.UpdateAsync(data);
                await tx.CommitAsync();
                retVal = true;

                dbSession.Close();
                dbSession.Dispose();
            }
            catch (Exception)
            {
                retVal = false;

                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }

                //await _logger.LogAsync("Error PersistService.Update", ex, null,
                //    new Dictionary<string, object> { { "data", data } });
            }

            return retVal;
        }

        /// <summary>
        /// Deletes the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public async Task<bool> Delete<T>(T data)
        {
            bool retVal;
            ITransaction tx = null;
            try
            {
                var dbSession = _sessionFactory.OpenSession();
                tx = dbSession.BeginTransaction();

                await dbSession.DeleteAsync(data);
                await tx.CommitAsync();
                retVal = true;

                dbSession.Close();
                dbSession.Dispose();
            }
            catch (Exception)
            {
                retVal = false;

                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }

                //await _logger.LogAsync("Error PersistService.Delete", ex, null,
                //    new Dictionary<string, object> { { "data", data } });
            }

            return retVal;
        }

        /// <summary>
        /// Finds the one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressionFunc">The expression function.</param>
        /// <returns></returns>
        public async Task<T> FindOne<T>(Expression<Func<T, bool>> expressionFunc) where T : class
        {
            return await Task.Run(() =>
            {
                var dbSession = _sessionFactory.OpenSession();
                var t = dbSession.QueryOver<T>().Where(expressionFunc).List().FirstOrDefault();

                dbSession.Close();
                dbSession.Dispose();

                return t;
            });
        }

        public async Task<List<T>> FindMany<T>(Expression<Func<T, bool>> expressionFunc) where T : class
        {
            return await Task.Run(() =>
            {
                var dbSession = _sessionFactory.OpenSession();
                var t = dbSession.QueryOver<T>().Where(expressionFunc).List().ToList();

                dbSession.Close();
                dbSession.Dispose();

                return t;
            });
        }

        public async Task<T> FindLast<T>(Expression<Func<T, bool>> expressionFunc, Expression<Func<T, object>> sortFunc) where T : class
        {
            return await Task.Run(() =>
            {
                var dbSession = _sessionFactory.OpenSession();
                var t = dbSession.QueryOver<T>().Where(expressionFunc)
                    .OrderBy(sortFunc).Desc.List().FirstOrDefault();

                dbSession.Close();
                dbSession.Dispose();

                return t;
            });
        }

        public async Task<bool> BatchSave<T>(List<T> data)
        {
            var retVal = false;
            ITransaction tx = null;
            
            try
            {
                var dbSession = _sessionFactory.OpenSession();
                tx = dbSession.BeginTransaction();
                var ctr = 0;

                foreach (var item in data)
                {
                    await dbSession.SaveAsync(item);
                    
                    ctr++;
                    if (ctr % 20 != 0)
                    {
                        continue;
                    }
                    
                    // flush a batch of inserts and release memory:
                    dbSession.Flush();
                    dbSession.Clear();
                }

                await tx.CommitAsync();

                dbSession.Close();
                dbSession.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }
                //await _logger.LogAsync("Error PersistService.Save", ex, null,
                //    new Dictionary<string, object> { { "data", data } });
            }

            return retVal;
        }
    }
}