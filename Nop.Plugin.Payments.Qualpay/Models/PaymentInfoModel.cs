using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Payments.Qualpay.Models
{
    /// <summary>
    /// Represents the Qualpay payment model
    /// </summary>
    public class PaymentInfoModel : BaseNopModel
    {
        #region Ctor

        public PaymentInfoModel()
        {
            ExpireMonths = new List<SelectListItem>();
            ExpireYears = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public bool IsGuest { get; set; }

        public string CardholderName { get; set; }
        
        public string CardNumber { get; set; }

        public string CardCode { get; set; }

        public string ExpireMonth { get; set; }
        public IList<SelectListItem> ExpireMonths { get; set; }
        
        public string ExpireYear { get; set; }
        public IList<SelectListItem> ExpireYears { get; set; }
        
        public bool SaveCardDetails { get; set; }
        
        public bool UseStoredCard { get; set; }

        #endregion
    }
}