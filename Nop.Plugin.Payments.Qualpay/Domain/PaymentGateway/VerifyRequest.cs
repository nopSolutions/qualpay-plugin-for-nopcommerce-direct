using System.Net;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents Qualpay Payment Gateway request to verify card data
    /// </summary>
    public class VerifyRequest : PaymentCardRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the payment gateway will store the cardholder data in the Card Vault and provide a card_id in the repsonse in an authorization, credit, force, sale, or verify requests. If the card_number or card_id in the request is already in the Card Vault, this flag instructs the payment gateway to update the associated data (e.g. avs_address, avs_zip, exp_date) if present.
        /// </summary>
        [JsonProperty(PropertyName = "tokenize")]
        public bool? IsTokenize { get; set; }

        /// <summary>
        /// Gets or sets a customer. In an authorization, credit, force, sale or verify message the merchant can send tokenize (set to true), either card_number or card_swipe, the desired customer_id, and the customer field and the payment gateway will create the customer data in the vault. Cannot be used to update an existing customer_id
        /// </summary>
        [JsonProperty(PropertyName = "customer")]
        public PaymentGatewayCustomer Customer { get; set; }

        /// <summary>
        /// Gets or sets a reference code supplied by the cardholder to the merchant.
        /// </summary>
        [JsonProperty(PropertyName = "customer_code")]
        public string CustomerCode { get; set; }

        /// <summary>
        /// Gets or sets a reference value that will be stored with the transaction data and included with transaction data in reports within Qualpay Manager. This value will also be attached to any lifecycle transactions (e.g. retrieval requests and chargebacks) that may occur.
        /// </summary>
        [JsonProperty(PropertyName = "merch_ref_num")]
        public string MerchantReferenceInfo { get; set; }

        /// <summary>
        /// Gets or sets the Mail Order Telephone Order (MOTO) transaction type.
        /// </summary>
        [JsonProperty(PropertyName = "moto_ecomm_ind")]
        public MotoTransactionType? MotoTransactionType { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Get a request path
        /// </summary>
        /// <returns>Request path</returns>
        public override string GetRequestPath() => $"pg/verify";

        /// <summary>
        /// Get a request method
        /// </summary>
        /// <returns>Request method</returns>
        public override string GetRequestMethod() => WebRequestMethods.Http.Post;

        #endregion
    }
}