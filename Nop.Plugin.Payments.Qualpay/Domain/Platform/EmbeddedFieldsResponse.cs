using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents response from Qualpay Embedded Fields
    /// </summary>
    public class EmbeddedFieldsResponse : PlatformResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets embedded key
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public EmbeddedKey EmbeddedKey { get; set; }

        #endregion
    }
}