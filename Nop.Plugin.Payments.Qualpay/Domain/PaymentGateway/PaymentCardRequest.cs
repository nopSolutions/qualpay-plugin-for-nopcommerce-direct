using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents base Qualpay Payment Gateway request with card data
    /// </summary>
    public abstract class PaymentCardRequest : PaymentGatewayRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value received from a tokenization request. This value may be used in place of a card number in messages requiring cardholder account data. 
        /// </summary>
        [JsonProperty(PropertyName = "card_id")]
        public string CardId { get; set; }

        /// <summary>
        /// Gets or sets a cardholder's card number. If this field is present in the request, the field card_swipe must NOT be present, the field exp_date must USUALLY be present, and the fields card_id and customer_id should NOT be present.
        /// </summary>
        [JsonProperty(PropertyName = "card_number")]
        public string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets an expiration date of cardholder card number. Required when the field card_number is present. If card_swipe is present in the request, this field must NOT be present. When card_id or customer_id is present in the request this field may also be present; if it is not, then the expiration date from the Card Vault will be used.
        /// </summary>
        [JsonProperty(PropertyName = "exp_date")]
        public string ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets a CVV2 or CID value from the signature panel on the back of the cardholder's card. If present during a request that requires authorization, the value will be sent to the issuer for validation.
        /// </summary>
        [JsonProperty(PropertyName = "cvv2")]
        public string Cvv2 { get; set; }

        /// <summary>
        /// Gets or sets a street address of the cardholder. If present, it will be included in the authorization request sent to the issuing bank.
        /// </summary>
        [JsonProperty(PropertyName = "avs_address")]
        public string AvsAddress { get; set; }

        /// <summary>
        /// Gets or sets a zip code of the cardholder. If present, it will be included in the authorization request sent to the issuing bank.
        /// </summary>
        [JsonProperty(PropertyName = "avs_zip")]
        public string AvsZip { get; set; }

        /// <summary>
        /// Gets or sets a cardholder's name. When provided in a tokenize request, the cardholder name will be stored in the Card Vault along with the cardholder card number and expiration date.
        /// </summary>
        [JsonProperty(PropertyName = "cardholder_name")]
        public string CardholderName { get; set; }

        /// <summary>
        /// Gets or sets track 1 or track 2 data magnetic stripe data. If the magnetic stripe reader provides both track 1 and track 2 data in a single read, it is the responsibility of the imeplementer to send data for only one of the two tracks.
        /// </summary>
        [JsonProperty(PropertyName = "card_swipe")]
        public string CardSwipe { get; set; }

        #endregion
    }
}