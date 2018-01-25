using System.Net;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents Qualpay Payment Gateway request to tokenize card data
    /// </summary>
    public class TokenizeRequest : PaymentCardRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether a single-use token to be generated. This token will expire in 10 minutes or when first used. This field defaults to \"false\".
        /// </summary>
        [JsonProperty(PropertyName = "single_use")]
        public bool? IsSingleUse { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Get a request path
        /// </summary>
        /// <returns>Request path</returns>
        public override string GetRequestPath() => $"pg/tokenize";

        /// <summary>
        /// Get a request method
        /// </summary>
        /// <returns>Request method</returns>
        public override string GetRequestMethod() => WebRequestMethods.Http.Post;

        #endregion
    }
}