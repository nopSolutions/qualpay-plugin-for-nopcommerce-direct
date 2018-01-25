
namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents enumeration of Mail Order Telephone Order (MOTO) transaction types
    /// </summary>
    public enum MotoTransactionType
    {
        /// <summary>
        /// Cardholder present
        /// </summary>
        Default = 0,

        /// <summary>
        /// One Time MOTO transaction
        /// </summary>
        OneTime = 1,

        /// <summary>
        /// Recurring 
        /// </summary>
        Recurring = 2,

        /// <summary>
        /// Installment
        /// </summary>
        Installment = 3,

        /// <summary>
        /// Full 3D-Secure transaction
        /// </summary>
        Full3DSecure = 5,

        /// <summary>
        /// Merchant 3D-Secure transaction
        /// </summary>
        Merchant3DSecure = 6,

        /// <summary>
        /// e-Commerce Channel Encrypted (SSL)
        /// </summary>
        Ssl = 7
    }
}