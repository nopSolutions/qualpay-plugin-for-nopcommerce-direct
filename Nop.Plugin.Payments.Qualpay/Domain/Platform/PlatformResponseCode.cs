using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents enumeration of Qualpay Platform response codes
    /// </summary>
    public enum PlatformResponseCode
    {
        /// <summary>
        /// The request was successful
        /// </summary>
        [JsonProperty(PropertyName = "0")]
        Success = 0,

        /// <summary>
        /// The request failed validation
        /// </summary>
        [JsonProperty(PropertyName = "2")]
        BadRequest = 2,

        /// <summary>
        /// The API Key being used does not have access to this resource
        /// </summary>
        [JsonProperty(PropertyName = "6")]
        InvalidCredentials = 6,

        /// <summary>
        /// The service you have called does not exist
        /// </summary>
        [JsonProperty(PropertyName = "7")]
        ResourceNotExists = 7,

        /// <summary>
        /// Unauthorized request
        /// </summary>
        [JsonProperty(PropertyName = "11")]
        Unauthorized = 11,

        /// <summary>
        /// There was a server problem processing the request
        /// </summary>
        [JsonProperty(PropertyName = "99")]
        InternalError = 99
    }
}