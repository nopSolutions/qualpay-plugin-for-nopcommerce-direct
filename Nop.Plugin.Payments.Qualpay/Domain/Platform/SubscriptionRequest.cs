using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents base request to subscription resources in Qualpay Recurring Billing
    /// </summary>
    public abstract class SubscriptionRequest : PlatformRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets Qualpay generated ID that identifies a subscription
        /// </summary>
        [JsonProperty(PropertyName = "subscription_id")]
        public long? SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets Unique ID assigned by Qualpay for a Merchant
        /// </summary>
        [JsonProperty(PropertyName = "merchant_id")]
        public long? MerchantId { get; set; }

        /// <summary>
        /// Gets or sets Unique ID that identifies a customer
        /// </summary>
        [JsonProperty(PropertyName = "customer_id")]
        public string CustomerId { get; set; }

        /// <summary>
        ///  Status of the subscription.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SubscriptionStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets unique profile ID to be used in payment gateway requests.
        /// </summary>
        [JsonProperty(PropertyName = "profile_id")]
        public string ProfileId { get; set; }

        /// <summary>
        /// Gets or sets Qualpay generated ID that identifies a Recurring Plan
        /// </summary>
        [JsonProperty(PropertyName = "plan_id")]
        public long? PlanId { get; set; }

        /// <summary>
        /// Gets or sets Name assigned by Merchant to a plan.
        /// </summary>
        [JsonProperty(PropertyName = "plan_name")]
        public string PlanName { get; set; }

        /// <summary>
        /// Gets or sets Code assigned by Merchant to a plan.
        /// </summary>
        [JsonProperty(PropertyName = "plan_code")]
        public string PlanCode { get; set; }

        /// <summary>
        /// Gets or sets First name of the Plan subscriber
        /// </summary>
        [JsonProperty(PropertyName = "customer_first_name")]
        public string CustomerFirstName { get; set; }

        /// <summary>
        /// Gets or sets Last name of the Plan subscriber
        /// </summary>
        [JsonProperty(PropertyName = "customer_last_name")]
        public string CustomerLastName { get; set; }

        /// <summary>
        /// Gets or sets Start Date of subscription. When adding a subscription, the start date should be in future. 
        /// </summary>
        [JsonProperty(PropertyName = "date_start")]
        public string DateStart { get; set; }

        /// <summary>
        /// Gets or sets Next Billing date of subscription. This field will be empty for cancelled and completed subscriptions.
        /// </summary>
        [JsonProperty(PropertyName = "date_next")]
        public string DateNext { get; set; }

        /// <summary>
        /// Gets or sets Date when the subscription will end. 
        /// </summary>
        [JsonProperty(PropertyName = "date_end")]
        public string DateEnd { get; set; }

        /// <summary>
        /// Gets or sets One-Time Fee amount. This fee will be charged when a subscription is added.
        /// </summary>
        [JsonProperty(PropertyName = "amt_setup")]
        public decimal? SetupAmount { get; set; }

        /// <summary>
        /// Gets or sets the date the customer will be billed the prorate amount, if first payment is prorated.
        /// </summary>
        [JsonProperty(PropertyName = "prorate_date_start")]
        public string ProrateDateStart { get; set; }

        /// <summary>
        /// Gets or sets the Prorate amount, if first payment is prorated.
        /// </summary>
        [JsonProperty(PropertyName = "prorate_amt")]
        public decimal? ProrateAmount { get; set; }

        /// <summary>
        /// Gets or sets the start date of the trial period, if there is a trial period.
        /// </summary>
        [JsonProperty(PropertyName = "trial_date_start")]
        public string TrialDateStart { get; set; }

        /// <summary>
        /// Gets or sets the end date of the trial period, if there is a trial period.
        /// </summary>
        [JsonProperty(PropertyName = "trial_date_end")]
        public string TrialDateEnd { get; set; }

        /// <summary>
        /// Gets or sets the amount billed during the trial period, if there is a trial period. Should be a positive amount. 
        /// </summary>
        [JsonProperty(PropertyName = "trial_amt")]
        public decimal? TrialAmount { get; set; }

        /// <summary>
        /// Gets or sets Date Regular billing cycle will start.
        /// </summary>
        [JsonProperty(PropertyName = "recur_date_start")]
        public string RecurringDateStart { get; set; }

        /// <summary>
        /// Gets or sets Date Regular billing cycle will end. 
        /// </summary>
        [JsonProperty(PropertyName = "recur_date_end")]
        public string RecurringDateEnd { get; set; }

        /// <summary>
        /// Gets or sets Regular Billing Amount. Amount should be a positive amount.
        /// </summary>
        [JsonProperty(PropertyName = "recur_amt")]
        public decimal? RecurringAmount { get; set; }

        /// <summary>
        /// Gets or sets Response from Gateway for one time set up fee transactions. Valid only when adding subscriptions with one time fee. 
        /// </summary>
        [JsonProperty(PropertyName = "response")]
        public TransactionResponse TransactionResponse { get; set; }

        /// <summary>
        /// Gets or sets the ISO numeric currency code for the transaction. 
        /// </summary>
        [JsonProperty(PropertyName = "tran_currency")]
        public int CurrencyIsoCode { get; set; }

        /// <summary>
        /// Gets or sets A short description of the plan, can be one off plan.
        /// </summary>
        [JsonProperty(PropertyName = "plan_desc")]
        public string PlanDescription { get; set; }

        /// <summary>
        /// Gets or sets the frequency of billing.
        /// </summary>
        [JsonProperty(PropertyName = "plan_frequency")]
        public PlanFrequency? PlanFrequency { get; set; }

        /// <summary>
        /// Gets or sets Number of billing cycles in the recurring transaction, -1 indicates bill until cancelled
        /// </summary>
        [JsonProperty(PropertyName = "plan_duration")]
        public int? PlanDuration { get; set; }

        /// <summary>
        ///  Gets or sets Number of months in a subscription cycle. Applicable only for monthly frequency.
        /// </summary>
        [JsonProperty(PropertyName = "interval")]
        public int? Interval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether subscriptions associated with a plan. True for subscriptions associated with a plan. False for one off subscriptions
        /// </summary>
        [JsonProperty(PropertyName = "subscription_on_plan")]
        public bool? IsSubscriptionOnPlan { get; set; }

        #endregion
    }
}