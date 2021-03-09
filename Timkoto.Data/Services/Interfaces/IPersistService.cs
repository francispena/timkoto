using System.Threading.Tasks;

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
        Task<double> Save<T>(T data);

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
    }
}