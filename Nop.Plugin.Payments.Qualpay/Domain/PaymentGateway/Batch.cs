using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.PaymentGateway
{
    /// <summary>
    /// Represents a result of the batch
    /// </summary>
    public class Batch
    {
        #region Properties

        /// <summary>
        /// Gets or sets a profile identifier. 
        /// </summary>
        [JsonProperty(PropertyName = "profile_id")]
        public string ProfileId { get; set; }

        /// <summary>
        /// Gets or sets a failure comment. 
        /// </summary>
        [JsonProperty(PropertyName = "comment")]
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets a batch number. 
        /// </summary>
        [JsonProperty(PropertyName = "batch_number")]
        public int? BatchId { get; set; }

        /// <summary>
        /// Gets or sets the total number of transactions
        /// </summary>
        [JsonProperty(PropertyName = "cnt_total")]
        public int? TransactionNumber { get; set; }

        /// <summary>
        /// Gets or sets the ISO numeric currency code for the transaction. 
        /// </summary>
        [JsonProperty(PropertyName = "tran_currency")]
        public int? CurrencyIsoCode { get; set; }

        /// <summary>
        /// Gets or sets purchases minus returns(can be a negative number).
        /// </summary>
        [JsonProperty(PropertyName = "amt_total")]
        public decimal? Amount { get; set; }

        #endregion
    }
}