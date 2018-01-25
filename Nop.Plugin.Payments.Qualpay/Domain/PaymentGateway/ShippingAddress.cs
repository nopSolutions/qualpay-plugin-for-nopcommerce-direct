using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents customer shipping address details
    /// </summary>
    public class ShippingAddress
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this shipping address is primary.
        /// </summary>
        [JsonProperty(PropertyName = "primary")]
        public bool? IsPrimary { get; set; }

        /// <summary>
        /// Gets or sets customer street and number, P.O. box, c/o.
        /// </summary>
        [JsonProperty(PropertyName = "shipping_addr1")]
        public string Address1 { get; set; }

        /// <summary>
        /// Gets or sets customer apartment, suite, unit, building, floor, etc.
        /// </summary>
        [JsonProperty(PropertyName = "shipping_addr2")]
        public string Address2 { get; set; }

        /// <summary>
        /// Gets or sets customer shipping city.
        /// </summary>
        [JsonProperty(PropertyName = "shipping_city")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets customer shipping country.
        /// </summary>
        [JsonProperty(PropertyName = "shipping_country")]
        public string CountryName { get; set; }

        /// <summary>
        /// Gets or sets ISO numeric country code for the shipping address.
        /// </summary>
        [JsonProperty(PropertyName = "shipping_country_code")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets business name if applicable. 
        /// </summary>
        [JsonProperty(PropertyName = "shipping_firm_name")]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets customer shipping first name.
        /// </summary>
        [JsonProperty(PropertyName = "shipping_first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets customer shipping last name.
        /// </summary>
        [JsonProperty(PropertyName = "shipping_last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets customer shipping state (abbreviated).
        /// </summary>
        [JsonProperty(PropertyName = "shipping_state")]
        public string StateCode { get; set; }

        /// <summary>
        /// Gets or sets customer shipping zip code.
        /// </summary>
        [JsonProperty(PropertyName = "shipping_zip")]
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets customer shipping zip+4 code if applicable.
        /// </summary>
        [JsonProperty(PropertyName = "shipping_zip4")]
        public string Zip4 { get; set; }

        #endregion
    }
}