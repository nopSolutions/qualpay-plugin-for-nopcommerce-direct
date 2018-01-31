using System.Net;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents Qualpay Recurring Billing request to get transactions by subscription
    /// </summary>
    public class GetSubscriptionTransactionsRequest : SubscriptionTransactionRequest
    {
        #region Methods

        /// <summary>
        /// Get a request path
        /// </summary>
        /// <returns>Request path</returns>
        public override string GetRequestPath() => $"platform/subscription/transactions/{SubscriptionId}";

        /// <summary>
        /// Get a request method
        /// </summary>
        /// <returns>Request method</returns>
        public override string GetRequestMethod() => WebRequestMethods.Http.Get;

        #endregion
    }
}