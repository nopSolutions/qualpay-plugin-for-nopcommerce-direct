using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Qualpay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            PaymentTransactionTypes = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Qualpay.Fields.MerchantId")]
        public string MerchantId { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("Plugins.Payments.Qualpay.Fields.SecurityKey")]
        public string SecurityKey { get; set; }
        public bool SecurityKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Qualpay.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        public int PaymentTransactionTypeId { get; set; }
        public bool PaymentTransactionTypeId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Qualpay.Fields.PaymentTransactionType")]
        public IList<SelectListItem> PaymentTransactionTypes { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Qualpay.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Qualpay.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}