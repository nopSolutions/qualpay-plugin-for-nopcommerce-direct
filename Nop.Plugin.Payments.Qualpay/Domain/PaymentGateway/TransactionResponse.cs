using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents response from Qualpay Payment Gateway to transaction request
    /// </summary>
    public class TransactionResponse : PaymentGatewayResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets a recurring transaction advice for MasterCard authorizations. M001 = New account information available. M002 = Try again later. M003 = Do not try again for recurring payments transaction. M004 = Token requirements not fulfilled for this token type. M021 = Recurring payment cancellation
        /// </summary>
        [JsonProperty(PropertyName = "merchant_advice_code")]
        public string MerchantAdviceCode { get; set; }

        #endregion
    }
}