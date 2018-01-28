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
        /// Gets or sets Tokenized Card number. If a card_id is not present in an add request, it is automatically created from the card_number. A card can be added only once. Duplicate cards are not permitted for a customer in the system. The card_id should be permanent. If this is a single use card_id, set the verify field to true which will make the card_id permanent.
        /// </summary>
        [JsonProperty(PropertyName = "card_id")]
        public string CardId { get; set; }

        /// <summary>
        /// Gets or sets The payment Card Number - masked if this is part of reponse. When adding payment information, a full card number or card id is required, A masked card number can be used if card_id is also included in the request. Once a card is added, the card number will always remain masked on any subsequent requests returning this field. A card can be added only once,  duplicate cards are not permitted for a customer in the system 
        /// </summary>
        [JsonProperty(PropertyName = "card_number")]
        public string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets Expiry Date in MMYY format.
        /// </summary>
        [JsonProperty(PropertyName = "exp_date")]
        public string ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets The CVV2 or CID value from the credit card. If present during a verify, the cvv will be sent to the issuer for validation. The CVV2 will not be stored in vault. The response will also not include CVV2.
        /// </summary>
        [JsonProperty(PropertyName = "cvv2")]
        public string Cvv2 { get; set; }

        /// <summary>
        /// Card Type. The card type is derived from the card number.
        /// </summary>
        [JsonProperty(PropertyName = "card_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardType? CardType { get; set; }

        /// <summary>
        /// Gets or sets Billing First Name. Can contain upto 32 characters.
        /// </summary>
        [JsonProperty(PropertyName = "billing_first_name")]
        public string BillingFirstName { get; set; }

        /// <summary>
        /// Gets or sets Billing Last Name. Can contain upto 32 characters. 
        /// </summary>
        [JsonProperty(PropertyName = "billing_last_name")]
        public string BillingLastName { get; set; }

        /// <summary>
        /// Gets or sets Business name on billing card, if applicable. 
        /// </summary>
        [JsonProperty(PropertyName = "billing_firm_name")]
        public string BillingCompany { get; set; }

        /// <summary>
        /// Gets or sets Billing Street Address. This address will also used for AVS verification if AVS verificaiton is enabled. 
        /// </summary>
        [JsonProperty(PropertyName = "billing_addr1")]
        public string BillingAddress1 { get; set; }

        /// <summary>
        /// Gets or sets Billing City.
        /// </summary>
        [JsonProperty(PropertyName = "billing_city")]
        public string BillingCity { get; set; }

        /// <summary>
        /// Gets or sets Billing State.
        /// </summary>
        [JsonProperty(PropertyName = "billing_state")]
        public string BillingStateCode { get; set; }

        /// <summary>
        /// Gets or sets Billing Zip.
        /// </summary>
        [JsonProperty(PropertyName = "billing_zip")]
        public string BillingZip { get; set; }

        /// <summary>
        /// Gets or sets Billing zip+4 code if applicable.
        /// </summary>
        [JsonProperty(PropertyName = "billing_zip4")]
        public string BillingZip4 { get; set; }

        /// <summary>
        /// Gets or sets Billing Country.
        /// </summary>
        [JsonProperty(PropertyName = "billing_country")]
        public string BillingCountryName { get; set; }

        /// <summary>
        /// Gets or sets ISO numeric country code for the billing address. Refer to <a href=\"/developer/api/reference#currency-codes\"target=\"_blank\">Currency Codes</a> for a list of country codes.
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