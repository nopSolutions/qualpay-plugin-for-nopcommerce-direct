using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Payments.Qualpay.Models;
using Nop.Web.Framework.Components;
using Nop.Services.Common;

namespace Nop.Plugin.Payments.Qualpay.Components
{
    [ViewComponent(Name = "Qualpay")]
    public class QualpayViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public QualpayViewComponent(IStoreContext storeContext, IWorkContext workContext)
        {
            this._storeContext = storeContext;
            this._workContext = workContext;
        }

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();

            //years
            for (var i = 0; i < 15; i++)
            {
                var year = (DateTime.Now.Year + i).ToString();
                model.ExpireYears.Add(new SelectListItem
                {
                    Text = year,
                    Value = year,
                });
            }

            //months
            for (var i = 1; i <= 12; i++)
            {
                model.ExpireMonths.Add(new SelectListItem
                {
                    Text = i.ToString("D2"),
                    Value = i.ToString(),
                });
            }

            //set postback values
            model.CardholderName = Request.Form["CardholderName"];
            model.CardNumber = Request.Form["CardNumber"];
            model.CardCode = Request.Form["CardCode"];
            var selectedMonth = model.ExpireMonths.FirstOrDefault(x => x.Value.Equals(Request.Form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedMonth != null)
                selectedMonth.Selected = true;
            var selectedYear = model.ExpireYears.FirstOrDefault(x => x.Value.Equals(Request.Form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedYear != null)
                selectedYear.Selected = true;

            //whether to save card details in Qualpay Vault
            var saveCardDetails = false;
            if (Request.Form.Keys.Contains("SaveCardDetails"))
                bool.TryParse(Request.Form["SaveCardDetails"][0], out saveCardDetails);
            model.SaveCardDetails = saveCardDetails;

            //check whether customer already has stored card
            var storedCardId = _workContext.CurrentCustomer.GetAttribute<string>("QualpayVaultCardId", _storeContext.CurrentStore.Id);
            var useStoredId = !string.IsNullOrEmpty(storedCardId);
            if (Request.Form.Keys.Contains("UseStoredCard"))
                bool.TryParse(Request.Form["UseStoredCard"][0], out useStoredId);
            model.UseStoredCard = useStoredId;

            return View("~/Plugins/Payments.Qualpay/Views/PaymentInfo.cshtml", model);
        }
    }
}
