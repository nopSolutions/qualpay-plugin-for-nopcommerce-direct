using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents webhook status enumeration
    /// </summary>
    public enum WebhookStatus
    {
        /// <summary>
        /// The webhook is active.
        /// </summary>
        [EnumMember(Value = "Active")]
        Active,

        /// <summary>
        /// The webhook was disabled by the user.
        /// </summary>
        [EnumMember(Value = "Disabled")]
        Disabled,

        /// <summary>
        /// The webhook was suspended by the system. A webhook is suspended when the system is unable to post a request for 48 hours.
        /// </summary>
        [EnumMember(Value = "Suspended")]
        Suspended
    }
}