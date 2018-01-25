using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents a single line item
    /// </summary>
    public class LineItem
    {
        #region Properties

        /// <summary>
        /// Gets or sets the count of items 
        /// </summary>
        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the description of item
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure (e.g.inches)
        /// </summary>
        [JsonProperty(PropertyName = "unit_of_measure")]
        public string MeasureUnit { get; set; }

        /// <summary>
        /// Gets or sets the product code or SKU number
        /// </summary>
        [JsonProperty(PropertyName = "product_code")]
        public string ProductCode { get; set; }

        /// <summary>
        /// Gets or sets the credit type
        /// </summary>
        [JsonProperty(PropertyName = "debit_credit_ind")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemCreditType CreditType { get; set; }

        /// <summary>
        /// Gets or sets the cost per unit
        /// </summary>
        [JsonProperty(PropertyName = "unit_cost")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the type of supply (Visa only)
        /// </summary>
        [JsonProperty(PropertyName = "type_of_supply ")]
        public string SupplyType { get; set; }

        /// <summary>
        /// Gets or sets the code used to categorize purchased item (Visa only)
        /// </summary>
        [JsonProperty(PropertyName = "commodity_code ")]
        public string CommodityCode { get; set; }

        #endregion
    }
}