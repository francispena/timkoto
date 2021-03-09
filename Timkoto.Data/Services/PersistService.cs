using NHibernate;
using System;
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
        public async Task<double> Save<T>(T data)
        {
            var retVal = 0d;
            ITransaction tx = null;
            try
            {
                var dbSession = _sessionFactory.OpenSession();
                tx = dbSession.BeginTransaction();

                var result = await dbSession.SaveAsync(data);
                await tx.CommitAsync();

                if (result != null)
                {
                    retVal = double.Parse(result.ToString());
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
    }
}