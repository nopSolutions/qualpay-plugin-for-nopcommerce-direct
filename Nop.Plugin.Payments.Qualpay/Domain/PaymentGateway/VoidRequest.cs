using System.Net;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents Qualpay Payment Gateway request to void transaction
    /// </summary>
    public class VoidRequest : PaymentGatewayRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets an identifier of the originally transaction. 
        /// </summary>
        [JsonIgnore]
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets an identifier of vendor to which this capture request applies.
        /// </summary>
        [JsonProperty(PropertyName = "vendor_id")]
        public long? VendorId { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Get a request path
        /// </summary>
        /// <returns>Request path</returns>
        public override string GetRequestPath() => $"pg/void/{TransactionId}";

        /// <summary>
        /// Get a request method
        /// </summary>
        /// <returns>Request method</returns>
        public override string GetRequestMethod() => WebRequestMethods.Http.Post;

        #endregion
    }
}