
namespace Nop.Plugin.Payments.Qualpay.Domain
{
    /// <summary>
    /// Represents base request to Qualpay services
    /// </summary>
    public abstract class QualpayRequest
    {
        #region Methods

        /// <summary>
        /// Get a request path
        /// </summary>
        /// <returns>Request path</returns>
        public abstract string GetRequestPath();

        /// <summary>
        /// Get a request method
        /// </summary>
        /// <returns>Request method</returns>
        public abstract string GetRequestMethod();

        #endregion
    }
}