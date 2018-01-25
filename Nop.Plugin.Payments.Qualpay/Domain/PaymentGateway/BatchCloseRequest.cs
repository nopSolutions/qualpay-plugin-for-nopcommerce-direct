using System.Net;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents Qualpay Payment Gateway request to close batch
    /// </summary>
    public class BatchCloseRequest : PaymentGatewayRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the ISO numeric currency code for the transaction. 
        /// </summary>
        [JsonProperty(PropertyName = "tran_currency")]
        public int CurrencyIsoCode { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Get a request path
        /// </summary>
        /// <returns>Request path</returns>
        public override string GetRequestPath() => "pg/batchClose";

        /// <summary>
        /// Get a request method
        /// </summary>
        /// <returns>Request method</returns>
        public override string GetRequestMethod() => WebRequestMethods.Http.Post;

        #endregion
    }
}