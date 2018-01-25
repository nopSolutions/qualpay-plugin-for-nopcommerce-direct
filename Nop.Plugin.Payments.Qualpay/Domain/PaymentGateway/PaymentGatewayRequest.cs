using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents base Qualpay Payment Gateway request
    /// </summary>
    public abstract class PaymentGatewayRequest : QualpayRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets a Qualpay merchant identifier. 
        /// </summary>
        [JsonProperty(PropertyName = "merchant_id")]
        public long? MerchantId { get; set; }

        /// <summary>
        /// Gets or sets the payment solution name you are requesting the Qualpay services
        /// </summary>
        [JsonProperty(PropertyName = "developer_id")]
        public string DeveloperId { get; set; }

        /// <summary>
        /// Gets or sets a location identifier. When a merchant has more than one location using the same currency, this value is used to identify the specific location
        /// </summary>
        [JsonProperty(PropertyName = "loc_id")]
        public string LocationId { get; set; }

        /// <summary>
        /// Gets or sets Payment Gateway profile should be used for the request.
        /// </summary>
        [JsonProperty(PropertyName = "profile_id")]
        public string ProfileId { get; set; }

        /// <summary>
        /// Gets or sets a session identifier (for internal use only).
        /// </summary>
        [JsonProperty(PropertyName = "session_id")]
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets a user identifier (for internal use only).
        /// </summary>
        [JsonProperty(PropertyName = "user_id")]
        public long? UserId { get; set; }

        /// <summary>
        /// Gets or sets the collection of field data that will be included with the transaction data reported in Qualpay Manager.
        /// </summary>
        [JsonProperty(PropertyName = "report_data")]
        public IEnumerable<KeyValuePair<string, string>> CustomReportFields { get; set; }

        #endregion
    }
}