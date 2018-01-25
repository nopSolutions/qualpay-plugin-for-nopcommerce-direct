using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents response from Qualpay Payment Gateway to tokenize request
    /// </summary>
    public class TokenizeResponse : PaymentGatewayResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets a cardholder's card number (truncated).
        /// </summary>
        [JsonProperty(PropertyName = "card_number")]
        public string CardNumber { get; set; }

        #endregion
    }
}