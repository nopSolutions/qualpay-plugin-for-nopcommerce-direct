using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents transaction status enumeration
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>
        /// Transaction is approved
        /// </summary>
        [EnumMember(Value = "A")]
        Approved,

        /// <summary>
        /// Transaction held
        /// </summary>
        [EnumMember(Value = "H")]
        Held,

        /// <summary>
        /// Transaction is captured
        /// </summary>
        [EnumMember(Value = "C")]
        Captured,

        /// <summary>
        /// Transaction is voided
        /// </summary>
        [EnumMember(Value = "V")]
        Voided,

        /// <summary>
        /// Transaction is cancelled
        /// </summary>
        [EnumMember(Value = "K")]
        Cancelled,

        /// <summary>
        /// Transaction is declined by issuer
        /// </summary>
        [EnumMember(Value = "D")]
        Declined,

        /// <summary>
        /// Transaction failures other than Issuer Declines
        /// </summary>
        [EnumMember(Value = "F")]
        Failed,

        /// <summary>
        /// Transaction settled
        /// </summary>
        [EnumMember(Value = "S")]
        Settled,

        /// <summary>
        /// Deposit sent
        /// </summary>
        [EnumMember(Value = "P")]
        DepositSent,

        /// <summary>
        /// Transaction settled, but will not be funded by Qualpay
        /// </summary>
        [EnumMember(Value = "N")]
        NotFunded,

        /// <summary>
        /// Transaction rejected
        /// </summary>
        [EnumMember(Value = "R")]
        Rejected
    }
}