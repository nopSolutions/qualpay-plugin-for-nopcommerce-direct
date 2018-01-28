using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents customer details
    /// </summary>
    public class PaymentGatewayCustomer
    {
        #region Properties

        /// <summary>
        /// Gets or sets customer billing address street.
        /// </summary>
        [JsonProperty(PropertyName = "billing_addr1")]
        public string BillingAddressAddress1 { get; set; }

        /// <summary>
        /// Gets or sets customer billing address, line 2.
        /// </summary>
        [JsonProperty(PropertyName = "billing_addr2")]
        public string BillingAddressAddress2 { get; set; }

        /// <summary>
        /// Gets or sets customer billing city.
        /// </summary>
        [JsonProperty(PropertyName = "billing_city")]
        public string BillingAddressCity { get; set; }

        /// <summary>
        /// Gets or sets customer billing country.
        /// </summary>
        [JsonProperty(PropertyName = "billing_country")]
        public string BillingAddressCountryName { get; set; }

        /// <summary>
        /// Gets or sets ISO numeric country code for the billing address.
        /// </summary>
        [JsonProperty(PropertyName = "billing_country_code")]
        public string BillingAddressCountryCode { get; set; }

        /// <summary>
        /// Gets or sets customer billing state (abbreviated).
        /// </summary>
        [JsonProperty(PropertyName = "billing_state")]
        public string BillingAddressStateCode { get; set; }

        /// <summary>
        /// Gets or sets customer billing zip code.
        /// </summary>
        [JsonProperty(PropertyName = "billing_zip")]
        public string BillingAddressZip { get; set; }

        /// <summary>
        /// Gets or sets customer billing zip+4 code if applicable.
        /// </summary>
        [JsonProperty(PropertyName = "billing_zip4")]
        public string BillingAddressZip4 { get; set; }

        /// <summary>
        /// Gets or sets customer e-mail address.
        /// </summary>
        [JsonProperty(PropertyName = "customer_email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets customer business name if applicable. 
        /// </summary>
        [JsonProperty(PropertyName = "customer_firm_name")]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets customer first name.
        /// </summary>
        [JsonProperty(PropertyName = "customer_first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets customer last name.
        /// </summary>
        [JsonProperty(PropertyName = "customer_last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets customer phone number.
        /// </summary>
        [JsonProperty(PropertyName = "customer_phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets list of shipping addresses for customer.
        /// </summary>
        [JsonProperty(PropertyName = "shipping_addresses")]
        public IEnumerable<ShippingAddress> ShippingAddresses { get; set; }

        #endregion
    }
}