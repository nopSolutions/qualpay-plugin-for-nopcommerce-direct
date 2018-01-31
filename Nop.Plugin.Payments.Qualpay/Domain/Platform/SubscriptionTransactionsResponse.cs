using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents subscription transactions response from Qualpay Recurring Billing
    /// </summary>
    public class SubscriptionTransactionsResponse : PlatformResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets transactions details
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public IEnumerable<SubscriptionTransaction> Transactions { get; set; }

        #endregion
    }
}