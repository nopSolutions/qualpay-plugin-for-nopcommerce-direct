using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents base response from Qualpay Platform
    /// </summary>
    public abstract class PlatformResponse : QualpayResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets a response code from API.
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PlatformResponseCode? ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets a short description of the API response code.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        
        #endregion
    }
}