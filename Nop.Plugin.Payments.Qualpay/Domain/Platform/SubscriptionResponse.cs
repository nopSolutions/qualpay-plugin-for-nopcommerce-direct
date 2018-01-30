using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents response from Qualpay Recurring Billing
    /// </summary>
    public class SubscriptionResponse : PlatformResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets Vault customer details
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public Subscription Subscription { get; set; }

        #endregion
    }
}