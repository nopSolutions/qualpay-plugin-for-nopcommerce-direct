using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents base request to Embedded Fields resources
    /// </summary>
    public abstract class EmbeddedKeyRequest : PlatformRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets a single use token used for loading embedded fields. The key will be invalidated when a card is successfully verified. The token will expire in 30 minutes. 
        /// </summary>
        [JsonProperty(PropertyName = "transient_key")]
        public string TransientKey { get; set; }

        /// <summary>
        /// Gets or sets unique ID assigned by Qualpay to a Merchant
        /// </summary>
        [JsonProperty(PropertyName = "merchant_id")]
        public long? MerchantId { get; set; }

        /// <summary>
        /// Gets or sets the key creation time stamp. 
        /// </summary>
        [JsonProperty(PropertyName = "db_timestamp")]
        public string Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the key will expire.
        /// </summary>
        [JsonProperty(PropertyName = "expiry_time")]
        public string ExpirationTime { get; set; }

        #endregion
    }
}