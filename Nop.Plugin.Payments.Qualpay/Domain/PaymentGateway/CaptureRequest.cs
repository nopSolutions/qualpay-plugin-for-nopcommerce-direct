using System.Net;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents Qualpay Payment Gateway request to capture transaction
    /// </summary>
    public class CaptureRequest : PaymentGatewayRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets an identifier of the originally transaction. 
        /// </summary>
        [JsonIgnore]
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the total amount of the transaction to capture (if different than original authorization amount). Partial captures are allowed here if the capture amount is less than the authorized amount.
        /// </summary>
        [JsonProperty(PropertyName = "amt_tran")]
        public decimal Amount { get; set; }

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
        public override string GetRequestPath() => $"pg/capture/{TransactionId}";

        /// <summary>
        /// Get a request method
        /// </summary>
        /// <returns>Request method</returns>
        public override string GetRequestMethod() => WebRequestMethods.Http.Post;

        #endregion
    }
}