using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Payments.Qualpay.Models;
using Nop.Services.Common;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Qualpay.Components
{
    [ViewComponent(Name = QualpayDefaults.ViewComponentName)]
    public class QualpayViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public QualpayViewComponent(IStoreContext storeContext, IWorkContext workContext)
        {
            this._storeContext = storeContext;
            this._workContext = workContext;
        }

        #endregion

        #region Methods

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();

            //prepare years
            for (var i = 0; i < 15; i++)
            {
                var year = (DateTime.Now.Year + i).ToString();
                model.ExpireYears.Add(new SelectListItem { Text = year, Value = year, });
            }

            //prepare months
            for (var i = 1; i <= 12; i++)
            {
                model.ExpireMonths.Add(new SelectListItem { Text = i.ToString("D2"), Value = i.ToString(), });
            }
            
            //whether current customer is guest
            model.IsGuest = _workContext.CurrentCustomer.IsGuest();
            
            //check whether customer already has stored card
            model.UseStoredCard = !model.IsGuest && 
                !string.IsNullOrEmpty(_workContext.CurrentCustomer.GetAttribute<string>("QualpayVaultCardId", _storeContext.CurrentStore.Id));

            return View("~/Plugins/Payments.Qualpay/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}