
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

        /// <summary>
        /// One page checkout route name
        /// </summary>
        public static string OnePageCheckoutRouteName => "CheckoutOnePage";

        /// <summary>
        /// Path to the Qualpay Embedded Fields js script
        /// </summary>
        public static string EmbeddedFieldsScriptPath => "https://app.qualpay.com/hosted/embedded/js/qp-embedded-sdk.min.js";

        /// <summary>
        /// Path to the Qualpay Embedded Fields css styles
        /// </summary>
        public static string EmbeddedFieldsStylePath => "https://app.qualpay.com/hosted/embedded/css/qp-embedded.css";

        /// <summary>
        /// Webhook label
        /// </summary>
        public static string WebhookLabel => "nopCommerce-plugin-webhook";

        /// <summary>
        /// Webhook route name
        /// </summary>
        public static string WebhookRouteName => "Plugin.Payments.Qualpay.Webhook";

        /// <summary>
        /// Subscription suspended webhook event
        /// </summary>
        public static string SubscriptionSuspendedWebhookEvent => "subscription_suspended";

        /// <summary>
        /// Subscription complete webhook event
        /// </summary>
        public static string SubscriptionCompleteWebhookEvent => "subscription_complete";

        /// <summary>
        /// Subscription payment success webhook event
        /// </summary>
        public static string SubscriptionPaymentSuccessWebhookEvent => "subscription_payment_success";

        /// <summary>
        /// Subscription payment failure webhook event
        /// </summary>
        public static string SubscriptionPaymentFailureWebhookEvent => "subscription_payment_failure";

        /// <summary>
        /// Validate URL webhook event
        /// </summary>
        public static string ValidateUrlWebhookEvent => "validate_url";

        /// <summary>
        /// Webhook signature header name
        /// </summary>
        public static string WebhookSignatureHeaderName => "x-qualpay-webhook-signature";
    }
}