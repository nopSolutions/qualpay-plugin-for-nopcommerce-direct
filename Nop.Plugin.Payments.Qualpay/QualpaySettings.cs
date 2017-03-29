using Nop.Core.Configuration;
using Nop.Plugin.Payments.Qualpay.Domain;

namespace Nop.Plugin.Payments.Qualpay
{
    /// <summary>
    /// Represents Qualpay payment gateway settings
    /// </summary>
    public class QualpaySettings : ISettings
    {
        /// <summary>
        /// Gets or sets a merchant identifier
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets a security key
        /// </summary>
        public string SecurityKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use sandbox (testing environment)
        /// </summary>
        public bool UseSandbox { get; set; }

        /// <summary>
        /// Gets or sets the payment transaction mode (authorization only or authorization and capture in a single request)
        /// </summary>
        public QualpayRequestType PaymentTransactionType { get; set; }

        /// <summary>
        /// Gets or sets an additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
    }
}
