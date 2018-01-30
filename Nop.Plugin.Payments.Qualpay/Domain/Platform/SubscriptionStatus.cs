using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents subscription status enumeration
    /// </summary>
    public enum SubscriptionStatus
    {
        /// <summary>
        /// Active
        /// </summary>
        [EnumMember(Value = "A")]
        Active,

        /// <summary>
        /// Complete
        /// </summary>
        [EnumMember(Value = "D")]
        Complete,

        /// <summary>
        /// Paused
        /// </summary>
        [EnumMember(Value = "P")]
        Paused,

        /// <summary>
        /// Cancelled
        /// </summary>
        [EnumMember(Value = "C")]
        Cancelled,

        /// <summary>
        /// Suspended
        /// </summary>
        [EnumMember(Value = "S")]
        Suspended
    }
}