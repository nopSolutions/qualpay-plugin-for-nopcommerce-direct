using System.Runtime.Serialization;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents card type enumeration
    /// </summary>
    public enum CardType
    {
        /// <summary>
        /// VISA
        /// </summary>
        [EnumMember(Value = "VS")]
        Visa,

        /// <summary>
        /// MasterCard
        /// </summary>
        [EnumMember(Value = "MC")]
        MasterCard,

        /// <summary>
        /// PayPal
        /// </summary>
        [EnumMember(Value = "PP")]
        PayPal,

        /// <summary>
        /// Discover
        /// </summary>
        [EnumMember(Value = "DS")]
        Discover,

        /// <summary>
        /// American Express
        /// </summary>
        [EnumMember(Value = "AM")]
        AmericanExpress
    }
}