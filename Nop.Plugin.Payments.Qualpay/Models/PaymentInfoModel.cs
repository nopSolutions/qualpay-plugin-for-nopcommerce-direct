using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Qualpay.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        public PaymentInfoModel()
        {
            ExpireMonths = new List<SelectListItem>();
            ExpireYears = new List<SelectListItem>();
        }

        [AllowHtml]
        [NopResourceDisplayName("Payment.CardholderName")]
        public string CardholderName { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("Payment.CardNumber")]
        public string CardNumber { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("Payment.ExpirationDate")]
        public string ExpireMonth { get; set; }
        public IList<SelectListItem> ExpireMonths { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("Payment.ExpirationDate")]
        public string ExpireYear { get; set; }
        public IList<SelectListItem> ExpireYears { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("Payment.CardCode")]
        public string CardCode { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Qualpay.SaveCardDetails")]
        public bool SaveCardDetails { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Qualpay.UseStoredCard")]
        public bool UseStoredCard { get; set; }
    }
}