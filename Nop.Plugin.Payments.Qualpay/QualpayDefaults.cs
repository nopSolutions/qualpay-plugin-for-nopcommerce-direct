
namespace Nop.Plugin.Payments.Qualpay
{
    /// <summary>
    /// Represents constants of the Qualpay payment plugin
    /// </summary>
    public class QualpayDefaults
    {
        /// <summary>
        /// Qualpay payment method system name
        /// </summary>
        public static string SystemName => "Payments.Qualpay";

        /// <summary>
        /// Name of the view component to display plugin in public store
        /// </summary>
        public const string ViewComponentName = "Qualpay";

        /// <summary>
        /// User agent using for requesting Qualpay services
        /// </summary>
        public static string UserAgent => "nopCommerce-plugin";

        /// <summary>
        /// nopCommerce developer application ID
        /// </summary>
        public static string DeveloperId => "nopCommerce";

        /// <summary>
        /// Numeric ISO code of the USD currency
        /// </summary>
        public static int UsdNumericIsoCode => 840;
    }
}