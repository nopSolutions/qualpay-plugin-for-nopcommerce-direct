using System.Net;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents Qualpay Customer Vault request to update customer card
    /// </summary>
    public class UpdateCustomerCardRequest : BillingCardRequest
    {
        #region Methods

        /// <summary>
        /// Get a request path
        /// </summary>
        /// <returns>Request path</returns>
        public override string GetRequestPath() => $"platform/vault/customer/{CustomerId}/billing";

        /// <summary>
        /// Get a request method
        /// </summary>
        /// <returns>Request method</returns>
        public override string GetRequestMethod() => WebRequestMethods.Http.Put;

        #endregion
    }
}