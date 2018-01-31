using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Payments.Qualpay.Models
{
    /// <summary>
    /// Represents the Qualpay configuration model
    /// </summary>
    public class ConfigurationModel : BaseNopModel
    {
        #region Ctor

        public ConfigurationModel()
        {
            PaymentTransactionTypes = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Qualpay.Fields.MerchantId")]
        public string MerchantId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Qualpay.Fields.SecurityKey")]
        [DataType(DataType.Password)]
        [NoTrim]
        public string SecurityKey { get; set; }
        public bool SecurityKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Qualpay.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Qualpay.Fields.UseEmbeddedFields")]
        public bool UseEmbeddedFields { get; set; }
        public bool UseEmbeddedFields_OverrideForStore { get; set; }

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

        #endregion
    }
}