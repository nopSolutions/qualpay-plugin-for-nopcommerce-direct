using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents response from Qualpay Webhook
    /// </summary>
    public class WebhookResponse : PlatformResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets webhook details
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public Webhook Webhook { get; set; }

        #endregion
    }
}