﻿using NHibernate;
using NHibernate.Transform;
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
                    retVal = long.Parse(result.ToString() ?? "0");
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
                    throw;
                }
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
            ITransaction tx = null;
            try
            {
                var dbSession = _sessionFactory.OpenSession();
                tx = dbSession.BeginTransaction();

                await dbSession.UpdateAsync(data);
                await tx.CommitAsync();

                dbSession.Close();
                dbSession.Dispose();
            }
            catch 
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }

                throw;
            }

            return true;
        }

        /// <summary>
        /// Deletes the specified data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public async Task<bool> Delete<T>(T data)
        {
            ITransaction tx = null;
            try
            {
                var dbSession = _sessionFactory.OpenSession();
                tx = dbSession.BeginTransaction();

                await dbSession.DeleteAsync(data);
                await tx.CommitAsync();

                dbSession.Close();
                dbSession.Dispose();
            }
            catch (Exception)
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }

                throw;
            }

            return true;
        }

        /// <summary>
        /// Finds the one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressionFunc">The expression function.</param>
        /// <returns></returns>
        public async Task<T> FindOne<T>(Expression<Func<T, bool>> expressionFunc) where T : class
        {
            var dbSession = _sessionFactory.OpenSession();
            var result = await dbSession.QueryOver<T>().Where(expressionFunc).ListAsync();

            var retVal = result.FirstOrDefault();

            dbSession.Close();
            dbSession.Dispose();

            return retVal;
        }

        public async Task<List<T>> FindMany<T>(Expression<Func<T, bool>> expressionFunc) where T : class
        {
            var dbSession = _sessionFactory.OpenSession();
            var result = await dbSession.QueryOver<T>().Where(expressionFunc).ListAsync();
            var retVal = result.ToList();

            dbSession.Close();
            dbSession.Dispose();

            return retVal;
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
            catch 
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }

                throw;
            }
        }

        public async Task<bool> ExecuteSql(string sqlStatement)
        {
            ITransaction tx = null;
            try
            {
                var dbSession = _sessionFactory.OpenSession();
                tx = dbSession.BeginTransaction();

                await dbSession.CreateSQLQuery(sqlStatement).ExecuteUpdateAsync();

                await tx.CommitAsync();

                dbSession.Close();
                dbSession.Dispose();

                return true;
            }
            catch 
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }

                throw;
            }
        }

        public async Task<List<T>> SqlQuery<T>(string sqlStatement)
        {
            ITransaction tx = null;
            try
            {
                var dbSession = _sessionFactory.OpenSession();
                tx = dbSession.BeginTransaction();

                var result = await dbSession.CreateSQLQuery(sqlStatement)
                    .SetResultTransformer(Transformers.AliasToBean<T>()).ListAsync<T>();

                var retVal = result.ToList();

                await tx.CommitAsync();

                dbSession.Close();
                dbSession.Dispose();

                return retVal;
            }
            catch
            {
                if (tx != null && tx.IsActive)
                {
                    await tx.RollbackAsync();
                }

                throw;
            }
        }

        public ISession GetSession()
        {
            return _sessionFactory.OpenSession();
        }
    }
}