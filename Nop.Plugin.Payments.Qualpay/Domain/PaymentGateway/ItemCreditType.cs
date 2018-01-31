using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents item credit types enumeration
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