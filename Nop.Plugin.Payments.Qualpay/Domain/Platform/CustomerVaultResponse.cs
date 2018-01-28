using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Qualpay.Domain.Platform
{
    /// <summary>
    /// Represents response from Qualpay Customer Vault
    /// </summary>
    public class CustomerVaultResponse : PlatformResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets Vault customer details
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public VaultCustomer VaultCustomer { get; set; }

        #endregion
    }
}