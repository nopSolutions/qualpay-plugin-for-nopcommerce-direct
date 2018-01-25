using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents enumeration of item credit types
    /// </summary>
    public enum ItemCreditType
    {
        /// <summary>
        /// Debit (sold)
        /// </summary>
        [JsonProperty(PropertyName = "D")]
        Debit,

        /// <summary>
        /// Credit (refunded)
        /// </summary>
        [JsonProperty(PropertyName = "C")]
        Credit
    }
}