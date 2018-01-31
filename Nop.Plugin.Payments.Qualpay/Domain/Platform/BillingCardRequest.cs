using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents base request to billing card resources in Qualpay Customer Vault
    /// </summary>
    public abstract class BillingCardRequest : PlatformRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets customer identifier.
        /// </summary>
        [JsonIgnore]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets tokenized card number. If a card_id is not present in an add request, it is automatically created from the card_number. A card can be added only once. Duplicate cards are not permitted for a customer in the system. The card_id should be permanent. If this is a single use card_id, set the verify field to true which will make the card_id permanent.
        /// </summary>
        [JsonProperty(PropertyName = "card_id")]
        public string CardId { get; set; }

        /// <summary>
        /// Gets or sets the payment card number - masked if this is part of reponse. When adding payment information, a full card number or card id is required, A masked card number can be used if card_id is also included in the request. Once a card is added, the card number will always remain masked on any subsequent requests returning this field. A card can be added only once,  duplicate cards are not permitted for a customer in the system 
        /// </summary>
        [JsonProperty(PropertyName = "card_number")]
        public string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets expiry date in MMYY format.
        /// </summary>
        [JsonProperty(PropertyName = "exp_date")]
        public string ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the CVV2 or CID value from the credit card. If present during a verify, the cvv will be sent to the issuer for validation. The CVV2 will not be stored in vault. The response will also not include CVV2.
        /// </summary>
        [JsonProperty(PropertyName = "cvv2")]
        public string Cvv2 { get; set; }

        /// <summary>
        /// Gets or sets card type. The card type is derived from the card number.
        /// </summary>
        [JsonProperty(PropertyName = "card_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardType? CardType { get; set; }

        /// <summary>
        /// Gets or sets billing first name. Can contain upto 32 characters.
        /// </summary>
        [JsonProperty(PropertyName = "billing_first_name")]
        public string BillingFirstName { get; set; }

        /// <summary>
        /// Gets or sets billing last name. Can contain upto 32 characters. 
        /// </summary>
        [JsonProperty(PropertyName = "billing_last_name")]
        public string BillingLastName { get; set; }

        /// <summary>
        /// Gets or sets business name on billing card, if applicable. 
        /// </summary>
        [JsonProperty(PropertyName = "billing_firm_name")]
        public string BillingCompany { get; set; }

        /// <summary>
        /// Gets or sets billing street address. This address will also used for AVS verification if AVS verificaiton is enabled. 
        /// </summary>
        [JsonProperty(PropertyName = "billing_addr1")]
        public string BillingAddress1 { get; set; }

        /// <summary>
        /// Gets or sets billing city.
        /// </summary>
        [JsonProperty(PropertyName = "billing_city")]
        public string BillingCity { get; set; }

        /// <summary>
        /// Gets or sets billing state.
        /// </summary>
        [JsonProperty(PropertyName = "billing_state")]
        public string BillingStateCode { get; set; }

        /// <summary>
        /// Gets or sets billing zip.
        /// </summary>
        [JsonProperty(PropertyName = "billing_zip")]
        public string BillingZip { get; set; }

        /// <summary>
        /// Gets or sets billing zip+4 code if applicable.
        /// </summary>
        [JsonProperty(PropertyName = "billing_zip4")]
        public string BillingZip4 { get; set; }

        /// <summary>
        /// Gets or sets billing country.
        /// </summary>
        [JsonProperty(PropertyName = "billing_country")]
        public string BillingCountryName { get; set; }

        /// <summary>
        /// Gets or sets ISO numeric country code for the billing address.
        /// </summary>
        [JsonProperty(PropertyName = "billing_country_code")]
        public string BillingCountryCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this card should be verified by the issuer before adding to Customer Vault. When this field is set to true, the customer will be added to vault either if the card verification successful or if card verification is not supported by the issuer.Default value is false and the card will not be verified before adding to vault 
        /// </summary>
        [JsonProperty(PropertyName = "verify")]
        public bool? Verify { get; set; }

        /// <summary>
        /// Gets or sets a date the card was last verified successfully.
        /// </summary>
        [JsonProperty(PropertyName = "verified_date")]
        public string VerifiedDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether its the default card. If there are multiple cards with primary true, only one of the card will be choosen to be the default card.
        /// </summary>
        [JsonProperty(PropertyName = "primary")]
        public bool? IsPrimary { get; set; }

        #endregion
    }
}